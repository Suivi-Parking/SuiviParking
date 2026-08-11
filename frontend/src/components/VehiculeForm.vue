<template>
    <div class="modal-backdrop" @click.self="$emit('cancel')">
        <div class="modal-card">
            <header class="modal-header">
                <h3>{{ mode === 'create' ? 'Nouveau véhicule' : 'Modifier le véhicule' }}</h3>
                <button class="btn-close" @click="$emit('cancel')">✕</button>
            </header>

            <form @submit.prevent="onSave" class="modal-body">
                <div class="grid-2">
                    <div class="field span-2">
                        <label>Immatriculation *</label>
                        <input v-model="form.immatriculation" placeholder="TUN-1234" :class="{ error: errors.immatriculation }" />
                        <span v-if="errors.immatriculation" class="error-msg">{{ errors.immatriculation }}</span>
                    </div>
                    <div class="field">
                        <label>Marque *</label>
                        <input v-model="form.marque" placeholder="Toyota, Peugeot..." :class="{ error: errors.marque }" />
                        <span v-if="errors.marque" class="error-msg">{{ errors.marque }}</span>
                    </div>
                    <div class="field">
                        <label>Modèle *</label>
                        <input v-model="form.modele" placeholder="Corolla, 208..." :class="{ error: errors.modele }" />
                        <span v-if="errors.modele" class="error-msg">{{ errors.modele }}</span>
                    </div>
                    <div class="field">
                        <label>Couleur *</label>
                        <input v-model="form.couleur" placeholder="Blanc, Noir..." :class="{ error: errors.couleur }" />
                        <span v-if="errors.couleur" class="error-msg">{{ errors.couleur }}</span>
                    </div>
                    <div class="field">
                        <label>Type *</label>
                        <select v-model="form.type" :class="{ error: errors.type }">
                            <option value="">Sélectionner...</option>
                            <option value="Berline">Berline</option>
                            <option value="Citadine">Citadine</option>
                            <option value="SUV">SUV</option>
                            <option value="Break">Break</option>
                            <option value="Utilitaire">Utilitaire</option>
                        </select>
                        <span v-if="errors.type" class="error-msg">{{ errors.type }}</span>
                    </div>
                    <div class="field span-2" style="position:relative">
                        <label>Employé *</label>
                        <input v-model="employeSearch"
                               type="text"
                               autocomplete="off"
                               placeholder="Rechercher par nom ou fonction..."
                               :class="{ error: errors.employeId }"
                               @focus="isEmployeListOpen = true"
                               @input="handleEmployeInput"
                               @blur="delayedClose" />
                        <ul v-if="isEmployeListOpen && employesFiltres.length" class="combo-list">
                            <li v-for="e in employesFiltres" :key="e.id"
                                @mousedown.prevent="selectEmploye(e)"
                                :class="{ selected: form.employeId === e.id }">
                                <strong>{{ e.nom }} {{ e.prenom }}</strong> — {{ e.fonction }}
                            </li>
                        </ul>
                        <div v-else-if="isEmployeListOpen && employeSearch && !employesFiltres.length" class="combo-empty">
                            Aucun employé actif trouvé.
                        </div>
                        <span v-if="errors.employeId" class="error-msg">{{ errors.employeId }}</span>
                    </div>
                    <div class="field" v-if="mode === 'edit'">
                        <label>Statut</label>
                        <select v-model="form.statut">
                            <option value="ACTIF">ACTIF</option>
                            <option value="INACTIF">INACTIF</option>
                        </select>
                    </div>
                </div>

                <div class="modal-actions">
                    <button type="button" class="btn-secondary" @click="$emit('cancel')">Annuler</button>
                    <button type="submit" class="btn-primary">{{ mode === 'create' ? 'Créer' : 'Enregistrer' }}</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script setup>
    import { ref, watch, computed, onMounted } from 'vue'
    import api from '../services/api'

    const props = defineProps({
        mode: { type: String, default: 'create' },
        vehicule: { type: Object, default: null }
    })
    const emit = defineEmits(['save', 'cancel'])
    const errors = ref({})

    const form = ref({
        immatriculation: '',
        marque: '',
        modele: '',
        couleur: '',
        type: '',
        employeId: 1,
        statut: 'ACTIF'
    })

    // ===== Sélecteur Employé (recherche par nom / fonction, au lieu de l'ID brut) =====
    const employes = ref([])
    const employeSearch = ref('')
    const isEmployeListOpen = ref(false)

    const employesFiltres = computed(() => {
        const q = employeSearch.value.trim().toLowerCase()
        if (!q) return employes.value
        return employes.value.filter(e =>
            `${e.nom} ${e.prenom}`.toLowerCase().includes(q) ||
            (e.fonction || '').toLowerCase().includes(q)
        )
    })

    function labelEmploye(e) {
        return `${e.nom} ${e.prenom} — ${e.fonction}`
    }

    function selectEmploye(e) {
        form.value.employeId = e.id
        employeSearch.value = labelEmploye(e)
        isEmployeListOpen.value = false
    }

    // Si le texte change sans re-selection, on invalide l'ID retenu pour eviter
    // d'envoyer un employeId qui ne correspond plus au texte affiche.
    function handleEmployeInput() {
        isEmployeListOpen.value = true
        form.value.employeId = null
    }

    function delayedClose() {
        setTimeout(() => { isEmployeListOpen.value = false }, 150)
    }

    onMounted(async () => {
        try {
            // Controleur "EmployeController" (singulier) => route /api/Employe
            const response = await api.get('/Employe', { params: { pageSize: 1000 } })
            const tousEmployes = response.data.items || response.data || []
            employes.value = tousEmployes.filter(e => (e.statut || '').toUpperCase() === 'ACTIF')
        } catch (e) {
            console.error('Echec du chargement des employés', e)
            employes.value = []
        }
    })

    watch(() => props.vehicule, (val) => {
        if (val) {
            form.value = {
                id: val.id,
                immatriculation: val.immatriculation,
                marque: val.marque,
                modele: val.modele,
                couleur: val.couleur,
                type: val.type,
                employeId: val.employeId,
                statut: val.statut
            }
            employeSearch.value = val.employeNom ? val.employeNom : ''
        } else {
            form.value = {
                immatriculation: '', marque: '', modele: '',
                couleur: '', type: '', employeId: null, statut: 'ACTIF'
            }
            employeSearch.value = ''
        }
        errors.value = {}
    }, { immediate: true })

    function validate() {
        errors.value = {}
        if (!form.value.immatriculation?.trim()) errors.value.immatriculation = '⚠️ L\'immatriculation est obligatoire'
        else if (!/^[A-Z]{2,3}-\d{3,4}$/i.test(form.value.immatriculation))
            errors.value.immatriculation = '⚠️ Format invalide (ex: TUN-1234)'
        if (!form.value.marque?.trim()) errors.value.marque = '⚠️ La marque est obligatoire'
        if (!form.value.modele?.trim()) errors.value.modele = '⚠️ Le modèle est obligatoire'
        if (!form.value.couleur?.trim()) errors.value.couleur = '⚠️ La couleur est obligatoire'
        if (!form.value.type) errors.value.type = '⚠️ Le type est obligatoire'
        if (!form.value.employeId || form.value.employeId <= 0)
            errors.value.employeId = '⚠️ L\'ID employé est invalide'
        return Object.keys(errors.value).length === 0
    }

    async function onSave() {
        if (!validate()) return
        emit('save', { ...form.value })
    }
</script>

<style scoped>
    /* ===== MODALE MODERNE (identique aux autres) ===== */
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
        width: 600px;
        max-width: 94%;
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
        align-items: center;
        position: sticky;
        top: 0;
        background: #fff;
        z-index: 1;
    }

        .modal-header h3 {
            margin: 0;
            font-weight: 600;
            font-size: 1.1rem;
            color: #0b1a2f;
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

    .grid-2 {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 1rem 1.2rem;
    }

    .span-2 {
        grid-column: span 2;
    }

    .field {
        display: flex;
        flex-direction: column;
    }

        .field label {
            font-size: 0.85rem;
            font-weight: 500;
            color: #1e293b;
            margin-bottom: 0.3rem;
        }

        .field input, .field select {
            padding: 0.6rem 0.9rem;
            border: 1px solid #e2e8f0;
            border-radius: 10px;
            font-size: 0.95rem;
            transition: border 0.2s, box-shadow 0.2s;
            background: #fafcfd;
        }

            .field input:focus, .field select:focus {
                border-color: #106ad7;
                outline: none;
                box-shadow: 0 0 0 4px rgba(16, 106, 215, 0.1);
                background: #fff;
            }

            .field input.error, .field select.error {
                border-color: #dc2626;
            }

    .error-msg {
        color: #dc2626;
        font-size: 0.8rem;
        margin-top: 0.2rem;
    }

    .modal-actions {
        display: flex;
        justify-content: flex-end;
        gap: 0.8rem;
        margin-top: 1.5rem;
    }

    .btn-secondary {
        padding: 0.5rem 1.2rem;
        border: 1px solid #e2e8f0;
        border-radius: 30px;
        background: #fff;
        color: #1e293b;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s;
    }

        .btn-secondary:hover {
            background: #f1f5f9;
        }

    .btn-primary {
        padding: 0.5rem 1.6rem;
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

    @media (max-width: 600px) {
        .grid-2 {
            grid-template-columns: 1fr;
        }

        .span-2 {
            grid-column: span 1;
        }
    }
</style>