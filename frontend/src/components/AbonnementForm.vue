<template>
    <div class="modal-backdrop" style="position:fixed;inset:0;display:flex;align-items:center;justify-content:center;z-index:60">
        <div class="modal" style="width:600px;background:white;border-radius:10px;box-shadow:0 10px 30px rgba(0,0,0,0.2);overflow:hidden">
            <header style="padding:16px;border-bottom:1px solid #eef2f6;display:flex;justify-content:space-between;align-items:center">
                <h3 style="margin:0">Nouvel abonnement</h3>
                <button type="button" @click="$emit('cancel')" style="border:none;background:transparent;font-size:18px;cursor:pointer">✖</button>
            </header>

            <form @submit.prevent="submit" style="padding:16px;">
                <div style="display:flex;gap:12px;flex-wrap:wrap">
                    <div style="flex-basis:100%">
                        <label class="block text-sm">Entreprise *</label>
                        <select v-model="form.EntrepriseId" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px">
                            <option :value="null" disabled>Sélectionner une entreprise...</option>
                            <option v-for="e in entreprises" :key="e.id" :value="e.id">{{ e.nom }}</option>
                        </select>
                        <div v-if="errors.EntrepriseId" class="text-sm" style="color:#dc2626">{{ errors.EntrepriseId }}</div>
                    </div>

                    <div style="flex:1;min-width:220px;position:relative">
                        <label class="block text-sm">Parking *</label>
                        <input v-model="parkingSearch"
                               type="text"
                               autocomplete="off"
                               placeholder="Rechercher par code ou nom..."
                               @focus="isParkingListOpen = true"
                               @input="isParkingListOpen = true"
                               @blur="handleParkingBlur"
                               style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
                        <ul v-if="isParkingListOpen && parkingsFiltres.length"
                            style="position:absolute;z-index:70;background:#fff;border:1px solid #cbd5e1;border-radius:6px;margin-top:2px;max-height:180px;overflow-y:auto;width:100%;list-style:none;padding:4px 0;box-shadow:0 6px 16px rgba(0,0,0,0.08)">
                            <li v-for="p in parkingsFiltres" :key="p.id"
                                @mousedown.prevent="selectParking(p)"
                                :style="{ padding: '8px 10px', cursor: 'pointer', background: form.ParkingId === p.id ? '#eff6ff' : '#fff' }">
                                <strong>{{ p.code }}</strong> — {{ p.nom }}
                            </li>
                        </ul>
                        <div v-else-if="isParkingListOpen && parkingSearch && !parkingsFiltres.length"
                             style="position:absolute;z-index:70;background:#fff;border:1px solid #cbd5e1;border-radius:6px;margin-top:2px;padding:8px 10px;width:100%;color:#94a3b8;font-size:0.85rem">
                            Aucun parking actif trouvé.
                        </div>
                        <div v-if="errors.ParkingId" class="text-sm" style="color:#dc2626">{{ errors.ParkingId }}</div>
                    </div>

                    <div style="flex:1;min-width:200px">
                        <label class="block text-sm">Type d'abonnement *</label>
                        <select v-model="form.TypeAbonnement" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px">
                            <option value="" disabled>Choisir...</option>
                            <option value="Mensuel">Mensuel</option>
                            <option value="Trimestriel">Trimestriel</option>
                            <option value="Annuel">Annuel</option>
                        </select>
                        <div v-if="errors.TypeAbonnement" class="text-sm" style="color:#dc2626">{{ errors.TypeAbonnement }}</div>
                    </div>

                    <div style="flex:1;min-width:200px">
                        <label class="block text-sm">Date de début *</label>
                        <input v-model="form.DateDebut" type="date" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
                        <div v-if="errors.DateDebut" class="text-sm" style="color:#dc2626">{{ errors.DateDebut }}</div>
                    </div>

                    <div style="flex:1;min-width:200px">
                        <label class="block text-sm">Date de fin *</label>
                        <input v-model="form.DateFin" type="date" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
                        <div v-if="errors.DateFin" class="text-sm" style="color:#dc2626">{{ errors.DateFin }}</div>
                    </div>

                    <div style="flex:1;min-width:200px">
                        <label class="block text-sm">Montant mensuel (DT) *</label>
                        <input v-model.number="form.MontantMensuel" type="number" min="0" step="0.01" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
                        <div v-if="errors.MontantMensuel" class="text-sm" style="color:#dc2626">{{ errors.MontantMensuel }}</div>
                    </div>
                </div>

                <div style="display:flex;justify-content:flex-end;gap:8px;margin-top:16px">
                    <button type="button" @click="$emit('cancel')" style="padding:8px 12px;border-radius:6px;border:1px solid #cbd5e1;background:#fff">Annuler</button>
                    <button type="submit" class="primary">Créer</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script setup>
    import { reactive, ref, computed, onMounted } from 'vue'

    const emit = defineEmits(['save', 'cancel'])

    const form = reactive({
        EntrepriseId: null,
        ParkingId: null,
        TypeAbonnement: '',
        DateDebut: '',
        DateFin: '',
        MontantMensuel: null,
    })

    const errors = reactive({})
    const entreprises = ref([])
    const parkings = ref([])

    // === Sélecteur de parking (recherche par code / nom) ===
    const parkingSearch = ref('')
    const isParkingListOpen = ref(false)

    const parkingsFiltres = computed(() => {
        const q = parkingSearch.value.trim().toLowerCase()
        if (!q) return parkings.value
        return parkings.value.filter(p =>
            p.code.toLowerCase().includes(q) || p.nom.toLowerCase().includes(q)
        )
    })

    function selectParking(p) {
        form.ParkingId = p.id
        parkingSearch.value = `${p.code} — ${p.nom}`
        isParkingListOpen.value = false
    }

    // Delai avant fermeture pour laisser le @mousedown.prevent d'un <li> s'executer
    // avant que le blur du champ ne referme la liste.
    function handleParkingBlur() {
        setTimeout(() => { isParkingListOpen.value = false }, 150)
    }

    // Recupere entreprises + parkings actifs pour les selects (pageSize eleve pour tout charger)
    onMounted(async () => {
        try {
            const res = await fetch('/api/entreprises?pageSize=1000')
            const json = await res.json()
            entreprises.value = json.items || json || []
        } catch (e) {
            console.error(e)
            entreprises.value = []
        }

        try {
            const res = await fetch('/api/parkings?pageSize=1000')
            const json = await res.json()
            const tousParkings = json.items || json || []
            // Seuls les parkings actifs et non archives sont proposes a la selection
            parkings.value = tousParkings.filter(p => p.statut === 'Actif' && !p.estSupprime)
        } catch (e) {
            console.error(e)
            parkings.value = []
        }
    })

    function validate() {
        errors.EntrepriseId = !form.EntrepriseId ? "L'entreprise est requise" : ''
        errors.ParkingId = !form.ParkingId ? 'Le parking est requis' : ''
        errors.TypeAbonnement = !form.TypeAbonnement ? 'Le type est requis' : ''
        errors.DateDebut = !form.DateDebut ? 'La date de début est requise' : ''
        errors.DateFin = !form.DateFin ? 'La date de fin est requise' : ''
        errors.MontantMensuel = form.MontantMensuel === null || form.MontantMensuel < 0 ? 'Montant invalide' : ''

        if (!errors.DateDebut && !errors.DateFin && form.DateFin <= form.DateDebut) {
            errors.DateFin = 'La date de fin doit être postérieure à la date de début'
        }

        return !Object.values(errors).some(Boolean)
    }

    function submit() {
        if (!validate()) return
        emit('save', { ...form })
    }
</script>

<style scoped>
    .modal-backdrop {
        background: rgba(15,23,42,0.4)
    }
</style>