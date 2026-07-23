<template>
    <div class="modal-backdrop" @click.self="$emit('close')">
        <div class="modal-card modal-wide">
            <header class="modal-header">
                <div>
                    <h3>{{ parking?.nom ?? 'Détail du parking' }}</h3>
                    <p v-if="parking" class="subtitle">{{ parking.code }} · {{ parking.adresse }}</p>
                </div>
                <button class="btn-close" @click="$emit('close')">✕</button>
            </header>

            <div class="modal-body">
                <div v-if="isLoading" class="state-message">⏳ Chargement...</div>
                <div v-else-if="errorMsg" class="state-message error">{{ errorMsg }}</div>

                <template v-else-if="parking">
                    <!-- Résumé -->
                    <div class="stats-grid">
                        <div class="stat-card">
                            <p class="stat-label">Capacité totale</p>
                            <p class="stat-value">{{ parking.capaciteMax }}</p>
                        </div>
                        <div class="stat-card">
                            <p class="stat-label">Places occupées</p>
                            <p class="stat-value">{{ parking.placesOccupees }}</p>
                        </div>
                        <div class="stat-card">
                            <p class="stat-label">Taux d'occupation</p>
                            <p class="stat-value">{{ parking.tauxOccupation }}%</p>
                        </div>
                    </div>

                    <!-- Liste des zones -->
                    <div class="section">
                        <h4 class="section-title">Zones</h4>
                        <div v-for="zone in parking.zones"
                             :key="zone.id"
                             class="zone-card"
                             :class="{ inactive: !zone.estActive }">
                            <template v-if="editingZoneId === zone.id">
                                <div class="edit-zone-row">
                                    <input v-model="editZoneForm.nom" class="edit-input" />
                                    <select v-model="editZoneForm.type" class="edit-select">
                                        <option v-for="t in typesZone" :key="t" :value="t">{{ t }}</option>
                                    </select>
                                    <input v-model.number="editZoneForm.nombrePlaces" type="number" min="0" class="edit-input-sm" />
                                </div>
                                <span v-if="editZoneError" class="error-msg">{{ editZoneError }}</span>
                                <div class="edit-actions">
                                    <button class="btn-secondary" @click="editingZoneId = null">Annuler</button>
                                    <button class="btn-primary" @click="saveEditZone(zone.id)">Enregistrer</button>
                                </div>
                            </template>

                            <template v-else>
                                <div class="zone-header">
                                    <div>
                                        <span class="zone-name">{{ zone.nom }}</span>
                                        <span class="zone-type">({{ zone.type }})</span>
                                        <span v-if="!zone.estActive" class="zone-inactive-badge">désactivée</span>
                                    </div>
                                    <div class="zone-actions">
                                        <button class="btn-icon-text" @click="toggleActive(zone)">
                                            {{ zone.estActive ? 'Désactiver' : 'Activer' }}
                                        </button>
                                        <button class="btn-icon-text" @click="startEditZone(zone)">Modifier</button>
                                        <button class="btn-icon-text danger" @click="supprimerZone(zone.id)">Supprimer</button>
                                    </div>
                                </div>

                                <div class="progress-wrapper">
                                    <div class="progress-bar-bg">
                                        <div class="progress-bar-fill"
                                             :class="barColor(zone.tauxOccupation)"
                                             :style="{ width: Math.min(zone.tauxOccupation, 100) + '%' }"></div>
                                    </div>
                                    <span class="progress-text">{{ zone.placesOccupees }}/{{ zone.nombrePlaces }} · {{ zone.tauxOccupation }}%</span>
                                </div>

                                <div class="occupation-adjust">
                                    <input v-model.number="occupationInputs[zone.id]"
                                           type="number"
                                           min="0"
                                           :max="zone.nombrePlaces"
                                           class="occupation-input" />
                                    <span class="occupation-label">places occupées</span>
                                    <button class="btn-update" @click="ajusterOccupation(zone)">Mettre à jour</button>
                                </div>
                            </template>
                        </div>

                        <p v-if="parking.zones.length === 0" class="empty-text">Aucune zone pour l'instant.</p>
                    </div>

                    <!-- Ajout de zone -->
                    <div class="section add-zone-section">
                        <h4 class="section-title">Ajouter une zone</h4>
                        <div class="add-zone-row">
                            <input v-model="newZone.nom" placeholder="Nom (ex: Zone C)" class="add-input" />
                            <select v-model="newZone.type" class="add-select">
                                <option v-for="t in typesZone" :key="t" :value="t">{{ t }}</option>
                            </select>
                            <input v-model.number="newZone.nombrePlaces" type="number" min="1" placeholder="Places" class="add-input-sm" />
                            <button class="btn-primary" @click="ajouterZone">Ajouter</button>
                        </div>
                        <span v-if="newZoneError" class="error-msg">{{ newZoneError }}</span>
                    </div>
                </template>
            </div>
        </div>
    </div>
</template>

<script setup>
    import { ref, reactive, onMounted } from 'vue'

    const props = defineProps({ parkingId: { type: Number, required: true } })
    const emit = defineEmits(['close', 'changed'])

    const parking = ref(null)
    const isLoading = ref(true)
    const errorMsg = ref('')

    const typesZone = ['Standard', 'Visiteurs', 'Residents', 'VIP', 'Livraison', 'Autre']

    const newZone = reactive({ nom: '', type: 'Standard', nombrePlaces: 0 })
    const newZoneError = ref('')

    const editingZoneId = ref(null)
    const editZoneForm = reactive({ nom: '', type: 'Standard', nombrePlaces: 0 })
    const editZoneError = ref('')

    const occupationInputs = reactive({})

    async function loadDetail() {
        isLoading.value = true
        errorMsg.value = ''
        try {
            const res = await fetch(`/api/parkings/${props.parkingId}`)
            if (!res.ok) throw new Error()
            parking.value = await res.json()
            parking.value.zones.forEach(z => {
                occupationInputs[z.id] = z.placesOccupees
            })
        } catch {
            errorMsg.value = 'Impossible de charger le détail de ce parking.'
        } finally {
            isLoading.value = false
        }
    }

    function barColor(taux) {
        if (taux > 90) return 'bg-danger'
        if (taux >= 70) return 'bg-warning'
        return 'bg-success'
    }

    async function ajouterZone() {
        newZoneError.value = ''
        if (!newZone.nom || newZone.nombrePlaces <= 0) {
            newZoneError.value = 'Nom et nombre de places (> 0) requis.'
            return
        }
        const res = await fetch(`/api/parkings/${props.parkingId}/zones`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newZone)
        })
        if (!res.ok) {
            newZoneError.value = (await res.text()) || 'Erreur lors de la création.'
            return
        }
        newZone.nom = ''
        newZone.type = 'Standard'
        newZone.nombrePlaces = 0
        await loadDetail()
        emit('changed')
    }

    function startEditZone(zone) {
        editingZoneId.value = zone.id
        editZoneForm.nom = zone.nom
        editZoneForm.type = zone.type
        editZoneForm.nombrePlaces = zone.nombrePlaces
        editZoneError.value = ''
    }

    async function saveEditZone(zoneId) {
        editZoneError.value = ''
        const res = await fetch(`/api/parkings/zones/${zoneId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(editZoneForm)
        })
        if (!res.ok) {
            editZoneError.value = (await res.text()) || 'Erreur lors de la modification.'
            return
        }
        editingZoneId.value = null
        await loadDetail()
        emit('changed')
    }

    async function supprimerZone(zoneId) {
        if (!confirm('Supprimer cette zone ? (impossible si c\'est la dernière du parking)')) return
        const res = await fetch(`/api/parkings/zones/${zoneId}`, { method: 'DELETE' })
        if (!res.ok) {
            alert((await res.text()) || 'Suppression impossible.')
            return
        }
        await loadDetail()
        emit('changed')
    }

    async function toggleActive(zone) {
        const res = await fetch(`/api/parkings/zones/${zone.id}/statut`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(!zone.estActive)
        })
        if (!res.ok) {
            alert((await res.text()) || 'Erreur lors du changement de statut.')
            return
        }
        await loadDetail()
        emit('changed')
    }

    async function ajusterOccupation(zone) {
        const valeur = occupationInputs[zone.id]
        const res = await fetch(`/api/parkings/zones/${zone.id}/occupation`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ placesOccupees: valeur })
        })
        if (!res.ok) {
            alert((await res.text()) || "Erreur lors de la mise à jour de l'occupation.")
            return
        }
        await loadDetail()
        emit('changed')
    }

    onMounted(loadDetail)
</script>

<style scoped>
    .modal-backdrop {
        position: fixed;
        inset: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        background: rgba(15, 23, 42, 0.5);
        backdrop-filter: blur(4px);
        z-index: 60;
    }

    .modal-card {
        width: 820px;
        max-width: 96%;
        background: #fff;
        border-radius: 20px;
        box-shadow: 0 20px 60px rgba(0,0,0,0.25);
        max-height: 90vh;
        overflow-y: auto;
    }

    .modal-header {
        padding: 1.2rem 1.5rem;
        border-bottom: 1px solid #eef2f6;
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        position: sticky;
        top: 0;
        background: #fff;
        z-index: 1;
    }

        .modal-header h3 {
            margin: 0;
            font-weight: 600;
            font-size: 1.2rem;
            color: #0b1a2f;
        }

    .subtitle {
        font-size: 0.85rem;
        color: #64748b;
        margin: 0.2rem 0 0;
    }

    .btn-close {
        background: none;
        border: none;
        font-size: 1.3rem;
        color: #94a3b8;
        cursor: pointer;
        padding: 0 4px;
        transition: color 0.2s;
    }

        .btn-close:hover {
            color: #1e293b;
        }

    .modal-body {
        padding: 1.5rem;
    }

    .state-message {
        padding: 2rem;
        text-align: center;
        color: #64748b;
    }

        .state-message.error {
            color: #dc2626;
        }

    .stats-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 1rem;
        margin-bottom: 1.5rem;
    }

    .stat-card {
        background: #f8fafc;
        border-radius: 12px;
        padding: 1rem;
        text-align: center;
    }

    .stat-label {
        font-size: 0.8rem;
        color: #94a3b8;
        margin: 0 0 0.2rem;
    }

    .stat-value {
        font-size: 1.4rem;
        font-weight: 700;
        color: #0b1a2f;
        margin: 0;
    }

    .section {
        border-top: 1px solid #eef2f6;
        padding-top: 1.2rem;
        margin-top: 1.2rem;
    }

    .section-title {
        font-size: 1rem;
        font-weight: 600;
        color: #0b1a2f;
        margin: 0 0 0.8rem;
    }

    .zone-card {
        background: #fafcfd;
        border: 1px solid #eef2f6;
        border-radius: 12px;
        padding: 0.8rem 1rem;
        margin-bottom: 0.8rem;
    }

        .zone-card.inactive {
            opacity: 0.6;
            background: #f1f5f9;
        }

    .zone-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        flex-wrap: wrap;
        gap: 0.5rem;
    }

    .zone-name {
        font-weight: 600;
    }

    .zone-type {
        font-size: 0.85rem;
        color: #64748b;
    }

    .zone-inactive-badge {
        font-size: 0.7rem;
        color: #94a3b8;
        background: #e2e8f0;
        padding: 0.1rem 0.5rem;
        border-radius: 20px;
        margin-left: 0.3rem;
    }

    .zone-actions {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
    }

    .btn-icon-text {
        background: none;
        border: none;
        font-size: 0.8rem;
        color: #106ad7;
        cursor: pointer;
        padding: 0.2rem 0.4rem;
        border-radius: 4px;
    }

        .btn-icon-text:hover {
            background: #eef6ff;
        }

        .btn-icon-text.danger {
            color: #dc2626;
        }

            .btn-icon-text.danger:hover {
                background: #fee2e2;
            }

    .progress-wrapper {
        display: flex;
        align-items: center;
        gap: 0.8rem;
        margin: 0.5rem 0;
    }

    .progress-bar-bg {
        flex: 1;
        height: 6px;
        background: #e2e8f0;
        border-radius: 999px;
        overflow: hidden;
    }

    .progress-bar-fill {
        height: 100%;
        border-radius: 999px;
        transition: width 0.3s ease;
    }

    .bg-success {
        background: #22c55e;
    }

    .bg-warning {
        background: #eab308;
    }

    .bg-danger {
        background: #ef4444;
    }

    .progress-text {
        font-size: 0.8rem;
        color: #64748b;
        white-space: nowrap;
    }

    .occupation-adjust {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        margin-top: 0.3rem;
    }

    .occupation-input {
        width: 70px;
        padding: 0.3rem 0.5rem;
        border: 1px solid #e2e8f0;
        border-radius: 6px;
        font-size: 0.85rem;
    }

    .occupation-label {
        font-size: 0.8rem;
        color: #94a3b8;
    }

    .btn-update {
        padding: 0.2rem 0.8rem;
        border-radius: 30px;
        border: 1px solid #106ad7;
        background: #fff;
        color: #106ad7;
        font-size: 0.75rem;
        font-weight: 500;
        cursor: pointer;
    }

        .btn-update:hover {
            background: #eef6ff;
        }

    .edit-zone-row {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
        margin-bottom: 0.5rem;
    }

    .edit-input {
        flex: 2;
        padding: 0.4rem 0.6rem;
        border: 1px solid #e2e8f0;
        border-radius: 6px;
        font-size: 0.85rem;
    }

    .edit-select {
        flex: 1;
        padding: 0.4rem 0.6rem;
        border: 1px solid #e2e8f0;
        border-radius: 6px;
        font-size: 0.85rem;
    }

    .edit-input-sm {
        width: 70px;
        padding: 0.4rem 0.6rem;
        border: 1px solid #e2e8f0;
        border-radius: 6px;
        font-size: 0.85rem;
    }

    .edit-actions {
        display: flex;
        gap: 0.5rem;
        margin-top: 0.3rem;
    }

    .error-msg {
        color: #dc2626;
        font-size: 0.8rem;
    }

    .empty-text {
        color: #94a3b8;
        font-size: 0.9rem;
        text-align: center;
        padding: 0.5rem;
    }

    .add-zone-section {
        border-top: 1px solid #eef2f6;
        padding-top: 1.2rem;
    }

    .add-zone-row {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
        align-items: center;
    }

    .add-input {
        flex: 2;
        padding: 0.5rem 0.8rem;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        font-size: 0.9rem;
    }

    .add-select {
        flex: 1;
        padding: 0.5rem 0.8rem;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        font-size: 0.9rem;
    }

    .add-input-sm {
        width: 80px;
        padding: 0.5rem 0.8rem;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        font-size: 0.9rem;
    }

    .btn-primary {
        padding: 0.5rem 1.2rem;
        border: none;
        border-radius: 30px;
        background: linear-gradient(135deg, #106ad7, #0b57c3);
        color: #fff;
        font-weight: 600;
        cursor: pointer;
        box-shadow: 0 4px 12px rgba(16, 106, 215, 0.3);
        transition: all 0.2s;
    }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 20px rgba(16, 106, 215, 0.4);
        }

    .btn-secondary {
        padding: 0.4rem 1rem;
        border: 1px solid #e2e8f0;
        border-radius: 30px;
        background: #fff;
        color: #1e293b;
        font-weight: 500;
        cursor: pointer;
    }

        .btn-secondary:hover {
            background: #f1f5f9;
        }

    @media (max-width: 600px) {
        .stats-grid {
            grid-template-columns: 1fr 1fr;
        }

        .add-zone-row {
            flex-direction: column;
        }

        .add-input, .add-select, .add-input-sm {
            width: 100%;
        }
    }
</style>