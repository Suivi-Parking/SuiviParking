<template>
    <div class="modal-backdrop" @click.self="$emit('cancel')">
        <div class="modal-card modal-wide">
            <header class="modal-header">
                <h3>{{ editing ? 'Modifier' : 'Nouveau' }} parking</h3>
                <button class="btn-close" @click="$emit('cancel')">✕</button>
            </header>

            <form @submit.prevent="submit" class="modal-body">
                <!-- Informations générales -->
                <div class="grid-2">
                    <div class="field span-2">
                        <label>Nom *</label>
                        <input v-model="form.nom" :class="{ error: errors.nom }" />
                        <span v-if="errors.nom" class="error-msg">{{ errors.nom }}</span>
                    </div>
                    <div class="field">
                        <label>Code unique *</label>
                        <input v-model="form.code" placeholder="ex: PARK-CENTRE" :class="{ error: errors.code }" />
                        <span v-if="errors.code" class="error-msg">{{ errors.code }}</span>
                    </div>
                    <div class="field">
                        <label>Statut</label>
                        <select v-model="form.statut">
                            <option value="Actif">Actif</option>
                            <option value="Inactif">Inactif</option>
                        </select>
                    </div>
                    <div class="field span-2">
                        <label>Adresse *</label>
                        <input v-model="form.adresse" :class="{ error: errors.adresse }" />
                        <span v-if="errors.adresse" class="error-msg">{{ errors.adresse }}</span>
                    </div>
                    <div class="field">
                        <label>Latitude</label>
                        <input v-model.number="form.latitude" type="number" step="0.000001" />
                    </div>
                    <div class="field">
                        <label>Longitude</label>
                        <input v-model.number="form.longitude" type="number" step="0.000001" />
                    </div>
                    <div class="field span-2">
                        <label>Image (URL)</label>
                        <input v-model="form.imageUrl" placeholder="https://..." />
                    </div>
                </div>

                <!-- Horaires -->
                <div class="section">
                    <label class="checkbox-label">
                        <input type="checkbox" v-model="form.ouvert24h24" />
                        Ouvert 24h/24
                    </label>
                    <div v-if="!form.ouvert24h24" class="grid-2">
                        <div class="field">
                            <label>Heure d'ouverture</label>
                            <input v-model="form.heureOuverture" type="time" />
                        </div>
                        <div class="field">
                            <label>Heure de fermeture</label>
                            <input v-model="form.heureFermeture" type="time" />
                        </div>
                    </div>
                    <span v-if="errors.horaires" class="error-msg">{{ errors.horaires }}</span>
                </div>

                <!-- Zones (création uniquement) -->
                <div v-if="!editing" class="section">
                    <div class="section-header">
                        <h4>Zones (au moins 1 requise)</h4>
                        <button type="button" class="btn-add-zone" @click="ajouterZone">+ Ajouter</button>
                    </div>
                    <div v-for="zone in zones" :key="zone.idLocal" class="zone-row">
                        <input v-model="zone.nom" placeholder="Nom" class="zone-input" />
                        <select v-model="zone.type" class="zone-select">
                            <option v-for="t in typesZone" :key="t" :value="t">{{ t }}</option>
                        </select>
                        <input v-model.number="zone.nombrePlaces" type="number" min="1" placeholder="Places" class="zone-input-sm" />
                        <button type="button"
                                class="btn-remove-zone"
                                :disabled="zones.length <= 1"
                                @click="supprimerZone(zone.idLocal)">
                            ✕
                        </button>
                    </div>
                    <span v-if="errors.zones" class="error-msg">{{ errors.zones }}</span>
                    <p class="help-text">La capacité totale sera calculée automatiquement.</p>
                </div>
                <p v-else class="help-text section">
                    Les zones se gèrent depuis la vue "Détail" du parking.
                </p>

                <div class="modal-actions">
                    <button type="button" class="btn-secondary" @click="$emit('cancel')">Annuler</button>
                    <button type="submit" class="btn-primary">{{ editing ? 'Enregistrer' : 'Créer' }}</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script setup>
    import { reactive, ref, watch } from 'vue'

    const props = defineProps({
        parking: { type: Object, default: null },
    })
    const emit = defineEmits(['save', 'cancel'])

    const editing = !!props.parking

    const form = reactive({
        nom: '',
        code: '',
        adresse: '',
        latitude: null,
        longitude: null,
        ouvert24h24: false,
        heureOuverture: '',
        heureFermeture: '',
        imageUrl: '',
        statut: 'Actif',
    })

    const typesZone = ['Standard', 'Visiteurs', 'Residents', 'VIP', 'Livraison', 'Autre']
    let prochainIdLocal = 1
    const zones = ref([{ idLocal: prochainIdLocal++, nom: '', type: 'Standard', nombrePlaces: null }])

    const errors = reactive({})

    watch(
        () => props.parking,
        (p) => {
            if (p) {
                form.nom = p.nom
                form.code = p.code
                form.adresse = p.adresse
                form.latitude = p.latitude
                form.longitude = p.longitude
                form.ouvert24h24 = p.ouvert24h24
                form.heureOuverture = p.heureOuverture?.slice(0, 5) ?? ''
                form.heureFermeture = p.heureFermeture?.slice(0, 5) ?? ''
                form.imageUrl = p.imageUrl ?? ''
                form.statut = p.statut
            }
        },
        { immediate: true }
    )

    function ajouterZone() {
        zones.value.push({ idLocal: prochainIdLocal++, nom: '', type: 'Standard', nombrePlaces: null })
    }
    function supprimerZone(idLocal) {
        if (zones.value.length <= 1) return
        zones.value = zones.value.filter(z => z.idLocal !== idLocal)
    }

    function normaliserHeure(valeur) {
        if (!valeur) return null
        return valeur.length === 5 ? `${valeur}:00` : valeur
    }

    function validate() {
        errors.nom = !form.nom ? 'Le nom est requis' : ''
        errors.code = !form.code ? 'Le code est requis' : ''
        errors.adresse = !form.adresse ? "L'adresse est requise" : ''
        if (!form.ouvert24h24) {
            errors.horaires = (!form.heureOuverture || !form.heureFermeture)
                ? "Horaires requis (ou cochez 'Ouvert 24h/24')"
                : ''
        } else {
            errors.horaires = ''
        }
        if (!editing) {
            const zonesInvalides = zones.value.some(z => !z.nom || !z.nombrePlaces || z.nombrePlaces <= 0)
            errors.zones = zonesInvalides ? 'Chaque zone doit avoir un nom et un nombre de places > 0' : ''
        }
        return !Object.values(errors).some(Boolean)
    }

    function submit() {
        if (!validate()) return

        const payload = {
            nom: form.nom,
            code: form.code,
            adresse: form.adresse,
            latitude: form.latitude,
            longitude: form.longitude,
            ouvert24h24: form.ouvert24h24,
            heureOuverture: form.ouvert24h24 ? null : normaliserHeure(form.heureOuverture),
            heureFermeture: form.ouvert24h24 ? null : normaliserHeure(form.heureFermeture),
            imageUrl: form.imageUrl || null,
            statut: form.statut,
        }

        if (!editing) {
            payload.zones = zones.value.map(z => ({
                nom: z.nom,
                type: z.type,
                nombrePlaces: z.nombrePlaces,
            }))
        }

        emit('save', payload)
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
        width: 720px;
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

            .field input.error {
                border-color: #dc2626;
            }

    .error-msg {
        color: #dc2626;
        font-size: 0.8rem;
        margin-top: 0.2rem;
    }

    .section {
        border-top: 1px solid #eef2f6;
        padding-top: 1rem;
        margin-top: 1rem;
    }

    .checkbox-label {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        font-size: 0.9rem;
        font-weight: 500;
        color: #1e293b;
    }

    .section-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 0.5rem;
    }

        .section-header h4 {
            margin: 0;
            font-size: 0.95rem;
            font-weight: 600;
            color: #0b1a2f;
        }

    .btn-add-zone {
        padding: 0.2rem 0.8rem;
        border-radius: 30px;
        border: 1px solid #106ad7;
        background: #fff;
        color: #106ad7;
        font-size: 0.8rem;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s;
    }

        .btn-add-zone:hover {
            background: #eef6ff;
        }

    .zone-row {
        display: flex;
        gap: 0.5rem;
        align-items: center;
        margin-bottom: 0.4rem;
    }

    .zone-input {
        flex: 2;
        padding: 0.4rem 0.6rem;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        font-size: 0.85rem;
    }

    .zone-select {
        flex: 1;
        padding: 0.4rem 0.6rem;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        font-size: 0.85rem;
    }

    .zone-input-sm {
        width: 70px;
        padding: 0.4rem 0.6rem;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        font-size: 0.85rem;
    }

    .btn-remove-zone {
        background: none;
        border: none;
        color: #94a3b8;
        font-size: 1.1rem;
        cursor: pointer;
        padding: 0 4px;
    }

        .btn-remove-zone:hover {
            color: #dc2626;
        }

        .btn-remove-zone:disabled {
            opacity: 0.3;
            cursor: not-allowed;
        }

    .help-text {
        font-size: 0.8rem;
        color: #94a3b8;
        margin-top: 0.3rem;
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

        .zone-row {
            flex-wrap: wrap;
        }

        .zone-input {
            flex: 1 1 100%;
        }
    }
</style>