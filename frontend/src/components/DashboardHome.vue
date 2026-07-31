<script setup>
    import { ref, reactive, computed, onMounted, onUnmounted, watch } from 'vue'
    import api from '../services/api'

    /* ============================================================
       ===================  Tableau de bord  =======================
       ============================================================ */
    const data = ref(null)
    const isLoading = ref(true)
    const lastUpdated = ref(null)
    let intervalId = null

    async function fetchDashboard() {
        try {
            const response = await api.get('/Dashboard')
            data.value = response.data
            lastUpdated.value = new Date()
        } finally {
            isLoading.value = false
        }
    }

    function formatMontant(m) {
        return new Intl.NumberFormat('fr-TN', { minimumFractionDigits: 2 }).format(m) + ' DT'
    }

    function formatHeure(d) {
        if (!d) return ''
        return d.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
    }

    function tauxStyle(taux) {
        if (taux >= 90) return { chip: 'bg-red-50 text-red-600', bar: 'bg-red-500' }
        if (taux >= 70) return { chip: 'bg-amber-50 text-amber-600', bar: 'bg-amber-500' }
        return { chip: 'bg-emerald-50 text-emerald-600', bar: 'bg-emerald-500' }
    }

    function impayeStyle(montant) {
        if (montant === 0) return 'bg-emerald-50 text-emerald-600'
        if (montant < 500) return 'bg-amber-50 text-amber-600'
        return 'bg-red-50 text-red-600'
    }

    function expiresStyle(count, total) {
        if (total === 0) return 'bg-slate-100 text-slate-500'
        const ratio = count / total
        if (ratio >= 0.3) return 'bg-red-50 text-red-600'
        if (ratio > 0) return 'bg-amber-50 text-amber-600'
        return 'bg-emerald-50 text-emerald-600'
    }

    onMounted(() => {
        fetchDashboard()
        intervalId = setInterval(fetchDashboard, 15000)
    })

    onUnmounted(() => {
        clearInterval(intervalId)
    })

    /* ============================================================
       ===============  Graphique des abonnements  ==================
       ============================================================ */
    const anneeCourante = new Date().getFullYear()
    const anneesDisponibles = [anneeCourante, anneeCourante - 1, anneeCourante - 2, anneeCourante - 3]
    const anneeSelectionnee = ref(anneeCourante)

    const donneesChart = ref(null) // { annee, mois: [{mois, nomMois, nouveaux, actifs, resiliations}] }
    const isLoadingChart = ref(true)
    const errorMsgChart = ref('')

    const svgRef = ref(null)
    const hover = reactive({ visible: false, x: 0, y: 0, mois: null })

    const series = [
        { key: 'nouveaux', label: 'Nouveaux', color: '#1B5FAE' },
        { key: 'actifs', label: 'Actifs', color: '#2E9E5B' },
        { key: 'resiliations', label: 'Résiliations', color: '#D64545' }
    ]

    // ===== Geometrie du graphique =====
    const largeur = 760
    const hauteur = 340
    const marge = { haut: 20, bas: 40, gauche: 40, droite: 20 }
    const largeurUtile = largeur - marge.gauche - marge.droite
    const hauteurUtile = hauteur - marge.haut - marge.bas

    const valeurMax = computed(() => {
        if (!donneesChart.value) return 10
        const max = Math.max(
            ...donneesChart.value.mois.flatMap(m => [m.nouveaux, m.actifs, m.resiliations])
        )
        return max === 0 ? 10 : Math.ceil(max * 1.15)
    })

    function xPourMois(index) {
        const pas = largeurUtile / 11
        return marge.gauche + index * pas
    }
    function yPourValeur(valeur) {
        return marge.haut + hauteurUtile - (valeur / valeurMax.value) * hauteurUtile
    }

    function pointsPolyline(cle) {
        if (!donneesChart.value) return ''
        return donneesChart.value.mois
            .map((m, i) => `${xPourMois(i)},${yPourValeur(m[cle])}`)
            .join(' ')
    }

    // Graduations Y (5 paliers)
    const graduationsY = computed(() => {
        const nb = 5
        return Array.from({ length: nb + 1 }, (_, i) => Math.round((valeurMax.value / nb) * i))
    })

    async function chargerAbonnements() {
        isLoadingChart.value = true
        errorMsgChart.value = ''
        try {
            const res = await fetch(`/api/abonnements/statistiques?annee=${anneeSelectionnee.value}`)
            if (!res.ok) throw new Error()
            donneesChart.value = await res.json()
        } catch {
            errorMsgChart.value = "Impossible de charger les statistiques."
            donneesChart.value = null
        } finally {
            isLoadingChart.value = false
        }
    }

    function onHoverMois(index, event) {
        const rect = svgRef.value.getBoundingClientRect()
        hover.visible = true
        hover.mois = donneesChart.value.mois[index]
        hover.x = xPourMois(index) / largeur * rect.width
        hover.y = yPourValeur(hover.mois.nouveaux) / hauteur * rect.height
    }
    function onLeaveMois() {
        hover.visible = false
    }

    // ===== Export =====
    function telechargerBlob(blob, nomFichier) {
        const url = URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = nomFichier
        a.click()
        URL.revokeObjectURL(url)
    }

    function exporterSVG() {
        const svgEl = svgRef.value
        const serialized = new XMLSerializer().serializeToString(svgEl)
        const blob = new Blob([serialized], { type: 'image/svg+xml' })
        telechargerBlob(blob, `abonnements-${anneeSelectionnee.value}.svg`)
    }

    function exporterPNG() {
        // On clone le SVG et on fixe explicitement width/height : sans cela,
        // le SVG sérialisé perd sa classe CSS "w-full h-auto" (qui n'existe
        // que dans le DOM) et le navigateur lui applique une taille par
        // défaut incohérente avec le viewBox, ce qui provoquait un rendu
        // recadré/zoomé sur l'export PNG.
        const svgEl = svgRef.value.cloneNode(true)
        svgEl.setAttribute('width', largeur)
        svgEl.setAttribute('height', hauteur)
        svgEl.setAttribute('xmlns', 'http://www.w3.org/2000/svg')

        const serialized = new XMLSerializer().serializeToString(svgEl)
        const svgBlob = new Blob([serialized], { type: 'image/svg+xml;charset=utf-8' })
        const url = URL.createObjectURL(svgBlob)

        const echelle = 2 // x2 pour une meilleure resolution
        const img = new Image()
        img.onload = () => {
            const canvas = document.createElement('canvas')
            canvas.width = largeur * echelle
            canvas.height = hauteur * echelle
            const ctx = canvas.getContext('2d')
            ctx.fillStyle = '#ffffff'
            ctx.fillRect(0, 0, canvas.width, canvas.height)
            // On force explicitement la taille de dessin (au lieu de se fier à
            // la taille "naturelle" de l'image) pour que le PNG corresponde
            // toujours exactement au viewBox du graphique.
            ctx.drawImage(img, 0, 0, canvas.width, canvas.height)
            URL.revokeObjectURL(url)

            canvas.toBlob(blob => {
                telechargerBlob(blob, `abonnements-${anneeSelectionnee.value}.png`)
            }, 'image/png')
        }
        img.onerror = () => {
            URL.revokeObjectURL(url)
            errorMsgChart.value = "Impossible d'exporter le graphique en PNG."
        }
        img.src = url
    }

    watch(anneeSelectionnee, chargerAbonnements)
    onMounted(chargerAbonnements)
</script>

<template>
    <div class="p-8">
        <div class="flex items-center justify-between mb-6">
            <div>
                <h1 class="text-2xl font-bold text-slate-800">Tableau de bord</h1>
                <p class="text-slate-500 text-sm mt-1">Vue d'ensemble de la plateforme</p>
            </div>
            <div class="flex items-center gap-3">
                <span v-if="lastUpdated" class="text-xs text-slate-400">
                    Mis à jour à {{ formatHeure(lastUpdated) }}
                </span>
                <button @click="fetchDashboard"
                        class="text-sm font-semibold text-primary hover:underline flex items-center gap-1">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M23 4v6h-6" />
                        <path d="M1 20v-6h6" />
                        <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15" />
                    </svg>
                    Actualiser
                </button>
            </div>
        </div>

        <div v-if="isLoading" class="text-slate-400 text-sm">Chargement du tableau de bord…</div>

        <div v-else-if="data" class="space-y-6">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Entreprises</p>
                    <p class="text-2xl font-bold text-slate-800 mt-2">{{ data.totalEntreprises }}</p>
                    <div class="flex gap-2 mt-3 text-xs">
                        <span class="px-2 py-1 rounded-full bg-emerald-50 text-emerald-600 font-semibold">
                            {{ data.entreprisesActives }} actives
                        </span>
                        <span class="px-2 py-1 rounded-full bg-slate-100 text-slate-500 font-semibold">
                            {{ data.entreprisesInactives }} inactives
                        </span>
                    </div>
                </div>

                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Abonnements actifs</p>
                    <p class="text-2xl font-bold text-emerald-600 mt-2">{{ data.abonnementsActifs }}</p>
                    <span class="inline-block mt-3 px-2 py-1 rounded-full text-xs font-semibold"
                          :class="expiresStyle(data.abonnementsExpires, data.abonnementsActifs + data.abonnementsExpires)">
                        {{ data.abonnementsExpires }} expirés
                    </span>
                </div>

                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Employés</p>
                    <p class="text-2xl font-bold text-slate-800 mt-2">{{ data.totalEmployes }}</p>
                    <p class="text-xs text-slate-400 mt-3">Collaborateurs enregistrés</p>
                </div>

                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Véhicules</p>
                    <p class="text-2xl font-bold text-slate-800 mt-2">{{ data.totalVehicules }}</p>
                    <p class="text-xs text-slate-400 mt-3">Véhicules enregistrés</p>
                </div>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Parkings</p>
                    <p class="text-2xl font-bold text-slate-800 mt-2">{{ data.totalParkings }}</p>
                    <p class="text-xs text-slate-400 mt-3">Sites gérés sur la plateforme</p>
                </div>

                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <div class="flex items-center justify-between">
                        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Taux d'occupation global</p>
                        <span class="px-2 py-1 rounded-full text-xs font-semibold"
                              :class="tauxStyle(data.tauxOccupationGlobal).chip">
                            {{ data.tauxOccupationGlobal >= 90 ? 'Saturé' : data.tauxOccupationGlobal >= 70 ? 'Élevé' : 'Normal' }}
                        </span>
                    </div>
                    <p class="text-2xl font-bold text-slate-800 mt-2">{{ data.tauxOccupationGlobal }}%</p>
                    <div class="w-full bg-slate-100 rounded-full h-2 mt-3">
                        <div class="h-2 rounded-full transition-all"
                             :class="tauxStyle(data.tauxOccupationGlobal).bar"
                             :style="{ width: Math.min(data.tauxOccupationGlobal, 100) + '%' }"></div>
                    </div>
                </div>
            </div>

            <div>
                <p class="text-sm font-semibold text-slate-600 mb-3">Revenus</p>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                    <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Revenu total</p>
                        <p class="text-2xl font-bold text-emerald-600 mt-2">{{ formatMontant(data.revenuTotal) }}</p>
                    </div>
                    <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Revenu du mois</p>
                        <p class="text-2xl font-bold text-primary mt-2">{{ formatMontant(data.revenuMensuel) }}</p>
                    </div>
                    <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Montant impayé</p>
                        <div class="flex items-center gap-2 mt-2">
                            <p class="text-2xl font-bold" :class="impayeStyle(data.montantImpaye).split(' ')[1]">
                                {{ formatMontant(data.montantImpaye) }}
                            </p>
                        </div>
                    </div>
                </div>
            </div>

            <!-- ============ Graphique : Évolution des abonnements ============ -->
            <div class="p-6 bg-panel rounded-xl shadow-soft border border-slate-100">
                <div class="flex items-center justify-between mb-4 flex-wrap gap-3">
                    <div>
                        <h3 class="font-semibold text-lg">Évolution des abonnements</h3>
                        <p class="text-sm text-gray-400">Nouveaux, actifs et résiliations sur 12 mois</p>
                    </div>

                    <div class="flex items-center gap-2">
                        <select v-model.number="anneeSelectionnee" class="px-3 py-2 border rounded-lg text-sm">
                            <option v-for="a in anneesDisponibles" :key="a" :value="a">{{ a }}</option>
                        </select>
                        <button class="px-3 py-2 rounded-lg border text-sm hover:bg-gray-50" @click="exporterPNG">
                            Exporter PNG
                        </button>
                        <button class="px-3 py-2 rounded-lg border text-sm hover:bg-gray-50" @click="exporterSVG">
                            Exporter SVG
                        </button>
                    </div>
                </div>

                <!-- Legende -->
                <div class="flex gap-4 mb-3 text-sm">
                    <div v-for="s in series" :key="s.key" class="flex items-center gap-1.5">
                        <span class="w-3 h-3 rounded-full" :style="{ background: s.color }" />
                        {{ s.label }}
                    </div>
                </div>

                <p v-if="isLoadingChart" class="text-gray-500">Chargement...</p>
                <p v-else-if="errorMsgChart" class="text-danger">{{ errorMsgChart }}</p>

                <div v-else class="relative">
                    <svg ref="svgRef"
                         :viewBox="`0 0 ${largeur} ${hauteur}`"
                         class="w-full h-auto"
                         style="background:#fff">
                        <!-- Grille horizontale + graduations Y -->
                        <g v-for="v in graduationsY" :key="v">
                            <line :x1="marge.gauche" :x2="largeur - marge.droite"
                                  :y1="yPourValeur(v)" :y2="yPourValeur(v)"
                                  stroke="#eef0f2" stroke-width="1" />
                            <text :x="marge.gauche - 8" :y="yPourValeur(v)" text-anchor="end" dominant-baseline="middle" class="fill-gray-400" font-size="11">
                                {{ v }}
                            </text>
                        </g>

                        <!-- Labels mois (axe X) -->
                        <text v-for="(m, i) in donneesChart.mois" :key="m.mois"
                              :x="xPourMois(i)" :y="hauteur - marge.bas + 20"
                              text-anchor="middle" class="fill-gray-500" font-size="11">
                            {{ m.nomMois }}
                        </text>

                        <!-- Courbes -->
                        <polyline v-for="s in series" :key="s.key"
                                  :points="pointsPolyline(s.key)"
                                  fill="none" :stroke="s.color" stroke-width="2.5" />

                        <!-- Points + zone de survol -->
                        <g v-for="(m, i) in donneesChart.mois" :key="'pts-' + m.mois">
                            <circle v-for="s in series" :key="s.key" :cx="xPourMois(i)" :cy="yPourValeur(m[s.key])" r="3.5" :fill="s.color" />
                            <rect :x="xPourMois(i) - (largeurUtile / 22)" :y="marge.haut"
                                  :width="largeurUtile / 11" :height="hauteurUtile"
                                  fill="transparent"
                                  @mouseenter="onHoverMois(i, $event)"
                                  @mouseleave="onLeaveMois"
                                  style="cursor:pointer" />
                        </g>
                    </svg>

                    <!-- Tooltip -->
                    <div v-if="hover.visible"
                         class="absolute bg-gray-900 text-white text-xs rounded-lg px-3 py-2 pointer-events-none shadow-lg"
                         :style="{ left: hover.x + 'px', top: Math.max(hover.y - 10, 10) + 'px', transform: 'translate(-50%, -100%)' }">
                        <div class="font-semibold mb-1">{{ hover.mois.nomMois }} {{ anneeSelectionnee }}</div>
                        <div v-for="s in series" :key="s.key" class="flex items-center gap-1.5">
                            <span class="w-2 h-2 rounded-full" :style="{ background: s.color }" />
                            {{ s.label }} : {{ hover.mois[s.key] }}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>