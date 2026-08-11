<template>
    <div class="acces-form-wrapper">
        <div class="acces-form-card">
            <h3 class="form-title">
                {{ mode === 'entree' ? '🚗 Autoriser une entrée' : '🚪 Enregistrer une sortie' }}
            </h3>

            <div class="toggle-group">
                <button type="button"
                        class="toggle-btn"
                        :class="{ active: mode === 'entree' }"
                        @click="mode = 'entree'">
                    Entrée
                </button>
                <button type="button"
                        class="toggle-btn"
                        :class="{ active: mode === 'sortie' }"
                        @click="mode = 'sortie'">
                    Sortie
                </button>
            </div>

            <form @submit.prevent="soumettre" class="form-body">
                <!-- Champs pour l'entrée -->
                <template v-if="mode === 'entree'">
                    <div class="field" style="position:relative">
                        <label>Véhicule (matricule)</label>
                        <input v-model="vehiculeSearch"
                               type="text"
                               autocomplete="off"
                               required
                               placeholder="Rechercher par immatriculation..."
                               @focus="isVehiculeListOpen = true"
                               @input="handleVehiculeInput"
                               @blur="delayedClose(isVehiculeListOpen)" />
                        <ul v-if="isVehiculeListOpen && vehiculesFiltres.length" class="combo-list">
                            <li v-for="v in vehiculesFiltres" :key="v.id"
                                @mousedown.prevent="selectVehicule(v)"
                                :class="{ selected: formEntree.vehiculeId === v.id }">
                                <strong>{{ v.immatriculation }}</strong> — {{ v.marque }} {{ v.modele }}
                            </li>
                        </ul>
                        <div v-else-if="isVehiculeListOpen && vehiculeSearch && !vehiculesFiltres.length" class="combo-empty">
                            Aucun véhicule actif trouvé.
                        </div>
                    </div>

                    <div class="field" style="position:relative">
                        <label>Parking (code)</label>
                        <input v-model="parkingSearch"
                               type="text"
                               autocomplete="off"
                               required
                               placeholder="Rechercher par code ou nom..."
                               @focus="isParkingListOpen = true"
                               @input="handleParkingInput"
                               @blur="delayedClose(isParkingListOpen)" />
                        <ul v-if="isParkingListOpen && parkingsFiltres.length" class="combo-list">
                            <li v-for="p in parkingsFiltres" :key="p.id"
                                @mousedown.prevent="selectParking(p)"
                                :class="{ selected: formEntree.parkingId === p.id }">
                                <strong>{{ p.code }}</strong> — {{ p.nom }}
                            </li>
                        </ul>
                        <div v-else-if="isParkingListOpen && parkingSearch && !parkingsFiltres.length" class="combo-empty">
                            Aucun parking actif trouvé.
                        </div>
                    </div>

                    <div class="field" style="position:relative">
                        <label>Abonnement <span class="optional">optionnel</span></label>
                        <input v-model="abonnementSearch"
                               type="text"
                               autocomplete="off"
                               placeholder="Rechercher par entreprise, type, parking..."
                               @focus="isAbonnementListOpen = true"
                               @input="handleAbonnementInput"
                               @blur="delayedClose(isAbonnementListOpen)" />
                        <ul v-if="isAbonnementListOpen && abonnementsFiltres.length" class="combo-list">
                            <li v-for="a in abonnementsFiltres" :key="a.id"
                                @mousedown.prevent="selectAbonnement(a)"
                                :class="{ selected: formEntree.abonnementId === a.id }">
                                {{ labelAbonnement(a) }}
                            </li>
                        </ul>
                        <div v-else-if="isAbonnementListOpen && abonnementSearch && !abonnementsFiltres.length" class="combo-empty">
                            Aucun abonnement actif trouvé.
                        </div>
                    </div>
                </template>

                <!-- Champs pour la sortie -->
                <template v-else>
                    <div class="field">
                        <label>Accès (ID)</label>
                        <input v-model.number="formSortie.accesId"
                               type="number"
                               required
                               min="1"
                               placeholder="ID de l'accès" />
                    </div>
                </template>

                <!-- Messages d'état -->
                <p v-if="erreur" class="message error">{{ erreur }}</p>
                <p v-if="succesMessage" class="message success">{{ succesMessage }}</p>

                <!-- Bouton de validation -->
                <button type="submit" class="btn-submit" :disabled="envoiEnCours">
                    {{ envoiEnCours ? 'Envoi...' : 'Valider' }}
                </button>
            </form>
        </div>
    </div>
</template>

<script setup>
    import { ref, reactive, computed, onMounted } from 'vue'
    import api from '../services/api'

    const emit = defineEmits(['acces-cree', 'acces-modifie'])

    const mode = ref('entree')
    const envoiEnCours = ref(false)
    const erreur = ref('')
    const succesMessage = ref('')

    const formEntree = reactive({
        vehiculeId: null,
        parkingId: null,
        abonnementId: null,
    })
    const formSortie = reactive({
        accesId: null,
    })

    // ===== Sélecteur Véhicule (recherche par immatriculation) =====
    const vehicules = ref([])
    const vehiculeSearch = ref('')
    const isVehiculeListOpen = ref(false)

    const vehiculesFiltres = computed(() => {
        const q = vehiculeSearch.value.trim().toLowerCase()
        if (!q) return vehicules.value
        return vehicules.value.filter(v =>
            (v.immatriculation || '').toLowerCase().includes(q) ||
            (v.marque || '').toLowerCase().includes(q) ||
            (v.modele || '').toLowerCase().includes(q)
        )
    })

    function selectVehicule(v) {
        formEntree.vehiculeId = v.id
        vehiculeSearch.value = `${v.immatriculation} — ${v.marque} ${v.modele}`.trim()
        isVehiculeListOpen.value = false
    }

    // Si le champ est vidé/modifié apres une selection, on invalide l'ID retenu
    // pour eviter d'envoyer un vehiculeId qui ne correspond plus au texte affiche.
    function handleVehiculeInput() {
        isVehiculeListOpen.value = true
        formEntree.vehiculeId = null
    }

    // ===== Sélecteur Parking (recherche par code) =====
    const parkings = ref([])
    const parkingSearch = ref('')
    const isParkingListOpen = ref(false)

    const parkingsFiltres = computed(() => {
        const q = parkingSearch.value.trim().toLowerCase()
        if (!q) return parkings.value
        return parkings.value.filter(p =>
            (p.code || '').toLowerCase().includes(q) ||
            (p.nom || '').toLowerCase().includes(q)
        )
    })

    function selectParking(p) {
        formEntree.parkingId = p.id
        parkingSearch.value = `${p.code} — ${p.nom}`
        isParkingListOpen.value = false
    }

    function handleParkingInput() {
        isParkingListOpen.value = true
        formEntree.parkingId = null
    }

    // ===== Sélecteur Abonnement (recherche par entreprise / type / parking, optionnel) =====
    const abonnements = ref([])
    const abonnementSearch = ref('')
    const isAbonnementListOpen = ref(false)

    const abonnementsFiltres = computed(() => {
        const q = abonnementSearch.value.trim().toLowerCase()
        if (!q) return abonnements.value
        return abonnements.value.filter(a =>
            (a.entrepriseNom || '').toLowerCase().includes(q) ||
            (a.typeAbonnement || '').toLowerCase().includes(q) ||
            (a.parkingNom || '').toLowerCase().includes(q)
        )
    })

    function labelAbonnement(a) {
        const parking = a.parkingNom ? ` · ${a.parkingNom}` : ''
        return `${a.entrepriseNom} — ${a.typeAbonnement}${parking}`
    }

    function selectAbonnement(a) {
        formEntree.abonnementId = a.id
        abonnementSearch.value = labelAbonnement(a)
        isAbonnementListOpen.value = false
    }

    function handleAbonnementInput() {
        isAbonnementListOpen.value = true
        formEntree.abonnementId = null
    }

    // Delai avant fermeture pour laisser le @mousedown.prevent d'un <li> s'executer
    // avant que le blur du champ ne referme la liste.
    function delayedClose(openRef) {
        setTimeout(() => { openRef.value = false }, 150)
    }

    function resetSelecteurs() {
        vehiculeSearch.value = ''
        parkingSearch.value = ''
        abonnementSearch.value = ''
    }

    onMounted(async () => {
        try {
            // Attention : le controleur backend s'appelle "VehiculeController" (singulier),
            // donc la route reelle est /api/Vehicule, pas /api/Vehicules.
            const response = await api.get('/Vehicule', { params: { pageSize: 1000 } })
            const tousVehicules = response.data.items || response.data || []
            // Comparaison insensible a la casse : la colonne Statut est un string libre
            // (pas un enum), et les donnees reelles semblent stockees en "Actif"
            // plutot qu'en "ACTIF" (valeur par defaut de l'entite Vehicule).
            vehicules.value = tousVehicules.filter(v => (v.statut || '').toUpperCase() === 'ACTIF')
        } catch (e) {
            console.error('Echec du chargement des vehicules', e)
            vehicules.value = []
        }

        try {
            const response = await api.get('/Parkings', { params: { pageSize: 1000 } })
            const tousParkings = response.data.items || response.data || []
            parkings.value = tousParkings.filter(p => (p.statut || '').toLowerCase() === 'actif' && !p.estSupprime)
        } catch (e) {
            console.error('Echec du chargement des parkings', e)
            parkings.value = []
        }

        try {
            const response = await api.get('/Abonnements', { params: { pageSize: 1000 } })
            const tousAbonnements = response.data.items || response.data || []
            abonnements.value = tousAbonnements.filter(a => (a.statut || '').toLowerCase() === 'actif')
        } catch (e) {
            console.error('Echec du chargement des abonnements', e)
            abonnements.value = []
        }
    })

    async function soumettre() {
        erreur.value = ''
        succesMessage.value = ''
        envoiEnCours.value = true

        try {
            if (mode.value === 'entree') {
                const reponse = await fetch('/api/acces/entree', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(formEntree),
                })
                const data = await reponse.json()
                if (!reponse.ok) {
                    erreur.value = data.message || 'Accès refusé.'
                    return
                }
                succesMessage.value = '✅ Entrée autorisée avec succès.'
                emit('acces-cree', data)
                // Réinitialisation
                formEntree.vehiculeId = null
                formEntree.parkingId = null
                formEntree.abonnementId = null
                resetSelecteurs()
            } else {
                const reponse = await fetch('/api/acces/sortie', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(formSortie),
                })
                const data = await reponse.json()
                if (!reponse.ok) {
                    erreur.value = data.message || data || "Erreur lors de l'enregistrement."
                    return
                }
                succesMessage.value = '✅ Sortie enregistrée avec succès.'
                emit('acces-modifie', data)
                formSortie.accesId = null
            }
        } catch {
            erreur.value = 'Erreur réseau : impossible de contacter le serveur.'
        } finally {
            envoiEnCours.value = false
        }
    }
</script>

<style scoped>
    /* Conteneur centré sur la page */
    .acces-form-wrapper {
        display: flex;
        justify-content: center;
        align-items: center;
        padding: 2rem 1rem;
        min-height: 40vh;
    }

    /* Carte du formulaire */
    .acces-form-card {
        width: 100%;
        max-width: 520px;
        background: #ffffff;
        border-radius: 24px;
        padding: 2rem 2rem 1.8rem;
        box-shadow: 0 8px 30px rgba(0, 0, 0, 0.06), 0 2px 8px rgba(16, 106, 215, 0.04);
        border: 1px solid #eef2f6;
        transition: transform 0.2s ease;
    }

        .acces-form-card:hover {
            transform: translateY(-2px);
        }

    .form-title {
        font-size: 1.3rem;
        font-weight: 700;
        color: #0b1a2f;
        margin: 0 0 1.2rem 0;
        letter-spacing: -0.01em;
    }

    /* Toggle (Entrée / Sortie) */
    .toggle-group {
        display: flex;
        gap: 0.5rem;
        margin-bottom: 1.5rem;
        background: #f1f5f9;
        padding: 0.3rem;
        border-radius: 30px;
    }

    .toggle-btn {
        flex: 1;
        padding: 0.5rem 0.8rem;
        border: none;
        border-radius: 30px;
        background: transparent;
        font-weight: 600;
        font-size: 0.9rem;
        color: #475569;
        cursor: pointer;
        transition: all 0.2s;
    }

        .toggle-btn.active {
            background: #106ad7;
            color: #fff;
            box-shadow: 0 2px 8px rgba(16, 106, 215, 0.2);
        }

        .toggle-btn:not(.active):hover {
            background: #e2e8f0;
        }

    /* Champs du formulaire */
    .form-body {
        display: flex;
        flex-direction: column;
        gap: 1.2rem;
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

    .optional {
        font-weight: 400;
        color: #94a3b8;
        font-size: 0.75rem;
    }

    .field input {
        padding: 0.6rem 0.9rem;
        border: 1px solid #e2e8f0;
        border-radius: 12px;
        font-size: 0.95rem;
        transition: border 0.2s, box-shadow 0.2s;
        background: #fafcfd;
    }

        .field input:focus {
            border-color: #106ad7;
            outline: none;
            box-shadow: 0 0 0 4px rgba(16, 106, 215, 0.1);
            background: #fff;
        }

    /* Liste déroulante des sélecteurs avec recherche (véhicule / parking / abonnement) */
    .combo-list {
        position: absolute;
        z-index: 70;
        top: 100%;
        left: 0;
        right: 0;
        margin-top: 4px;
        background: #fff;
        border: 1px solid #e2e8f0;
        border-radius: 12px;
        max-height: 200px;
        overflow-y: auto;
        list-style: none;
        padding: 4px 0;
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
    }

        .combo-list li {
            padding: 0.5rem 0.9rem;
            font-size: 0.85rem;
            cursor: pointer;
        }

            .combo-list li:hover,
            .combo-list li.selected {
                background: #eff6ff;
            }

    .combo-empty {
        position: absolute;
        z-index: 70;
        top: 100%;
        left: 0;
        right: 0;
        margin-top: 4px;
        background: #fff;
        border: 1px solid #e2e8f0;
        border-radius: 12px;
        padding: 0.5rem 0.9rem;
        font-size: 0.8rem;
        color: #94a3b8;
    }

    /* Messages */
    .message {
        font-size: 0.9rem;
        padding: 0.5rem 0.8rem;
        border-radius: 10px;
        margin: 0;
    }

        .message.error {
            background: #fee2e2;
            color: #b91c1c;
        }

        .message.success {
            background: #dcfce7;
            color: #15803d;
        }

    /* Bouton de validation */
    .btn-submit {
        padding: 0.7rem;
        border: none;
        border-radius: 30px;
        background: linear-gradient(135deg, #106ad7, #0b57c3);
        color: #fff;
        font-weight: 600;
        font-size: 1rem;
        cursor: pointer;
        transition: all 0.2s;
        box-shadow: 0 4px 12px rgba(16, 106, 215, 0.25);
        margin-top: 0.2rem;
    }

        .btn-submit:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 8px 20px rgba(16, 106, 215, 0.35);
        }

        .btn-submit:disabled {
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }
</style>