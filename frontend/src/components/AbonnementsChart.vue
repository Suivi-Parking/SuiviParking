<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'

const anneeCourante = new Date().getFullYear()
const anneesDisponibles = [anneeCourante, anneeCourante - 1, anneeCourante - 2, anneeCourante - 3]
const anneeSelectionnee = ref(anneeCourante)

const donnees = ref(null) // { annee, mois: [{mois, nomMois, nouveaux, actifs, resiliations}] }
const isLoading = ref(true)
const errorMsg = ref('')

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
  if (!donnees.value) return 10
  const max = Math.max(
    ...donnees.value.mois.flatMap(m => [m.nouveaux, m.actifs, m.resiliations])
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
  if (!donnees.value) return ''
  return donnees.value.mois
    .map((m, i) => `${xPourMois(i)},${yPourValeur(m[cle])}`)
    .join(' ')
}

// Graduations Y (5 paliers)
const graduationsY = computed(() => {
  const nb = 5
  return Array.from({ length: nb + 1 }, (_, i) => Math.round((valeurMax.value / nb) * i))
})

async function charger() {
  isLoading.value = true
  errorMsg.value = ''
  try {
    const res = await fetch(`/api/abonnements/statistiques?annee=${anneeSelectionnee.value}`)
    if (!res.ok) throw new Error()
    donnees.value = await res.json()
  } catch {
    errorMsg.value = "Impossible de charger les statistiques."
    donnees.value = null
  } finally {
    isLoading.value = false
  }
}

function onHoverMois(index, event) {
  const rect = svgRef.value.getBoundingClientRect()
  hover.visible = true
  hover.mois = donnees.value.mois[index]
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
  const svgEl = svgRef.value
  const serialized = new XMLSerializer().serializeToString(svgEl)
  const svgBlob = new Blob([serialized], { type: 'image/svg+xml;charset=utf-8' })
  const url = URL.createObjectURL(svgBlob)

  const img = new Image()
  img.onload = () => {
    const canvas = document.createElement('canvas')
    canvas.width = largeur * 2 // x2 pour une meilleure resolution
    canvas.height = hauteur * 2
    const ctx = canvas.getContext('2d')
    ctx.fillStyle = '#ffffff'
    ctx.fillRect(0, 0, canvas.width, canvas.height)
    ctx.scale(2, 2)
    ctx.drawImage(img, 0, 0)
    URL.revokeObjectURL(url)

    canvas.toBlob(blob => {
      telechargerBlob(blob, `abonnements-${anneeSelectionnee.value}.png`)
    }, 'image/png')
  }
  img.src = url
}

watch(anneeSelectionnee, charger)
onMounted(charger)
</script>

<template>
  <div class="p-6 bg-panel rounded-xl">
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

    <p v-if="isLoading" class="text-gray-500">Chargement...</p>
    <p v-else-if="errorMsg" class="text-danger">{{ errorMsg }}</p>

    <div v-else class="relative">
      <svg
        ref="svgRef"
        :viewBox="`0 0 ${largeur} ${hauteur}`"
        class="w-full h-auto"
        style="background:#fff"
      >
        <!-- Grille horizontale + graduations Y -->
        <g v-for="v in graduationsY" :key="v">
          <line
            :x1="marge.gauche" :x2="largeur - marge.droite"
            :y1="yPourValeur(v)" :y2="yPourValeur(v)"
            stroke="#eef0f2" stroke-width="1"
          />
          <text :x="marge.gauche - 8" :y="yPourValeur(v)" text-anchor="end" dominant-baseline="middle" class="fill-gray-400" font-size="11">
            {{ v }}
          </text>
        </g>

        <!-- Labels mois (axe X) -->
        <text
          v-for="(m, i) in donnees.mois" :key="m.mois"
          :x="xPourMois(i)" :y="hauteur - marge.bas + 20"
          text-anchor="middle" class="fill-gray-500" font-size="11"
        >
          {{ m.nomMois }}
        </text>

        <!-- Courbes -->
        <polyline
          v-for="s in series" :key="s.key"
          :points="pointsPolyline(s.key)"
          fill="none" :stroke="s.color" stroke-width="2.5"
        />

        <!-- Points + zone de survol -->
        <g v-for="(m, i) in donnees.mois" :key="'pts-' + m.mois">
          <circle v-for="s in series" :key="s.key" :cx="xPourMois(i)" :cy="yPourValeur(m[s.key])" r="3.5" :fill="s.color" />
          <rect
            :x="xPourMois(i) - (largeurUtile / 22)" :y="marge.haut"
            :width="largeurUtile / 11" :height="hauteurUtile"
            fill="transparent"
            @mouseenter="onHoverMois(i, $event)"
            @mouseleave="onLeaveMois"
            style="cursor:pointer"
          />
        </g>
      </svg>

      <!-- Tooltip -->
      <div
        v-if="hover.visible"
        class="absolute bg-gray-900 text-white text-xs rounded-lg px-3 py-2 pointer-events-none shadow-lg"
        :style="{ left: hover.x + 'px', top: Math.max(hover.y - 10, 10) + 'px', transform: 'translate(-50%, -100%)' }"
      >
        <div class="font-semibold mb-1">{{ hover.mois.nomMois }} {{ anneeSelectionnee }}</div>
        <div v-for="s in series" :key="s.key" class="flex items-center gap-1.5">
          <span class="w-2 h-2 rounded-full" :style="{ background: s.color }" />
          {{ s.label }} : {{ hover.mois[s.key] }}
        </div>
      </div>
    </div>
  </div>
</template>
