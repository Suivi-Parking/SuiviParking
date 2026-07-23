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
                    <div class="field">
                        <label>Véhicule (ID)</label>
                        <input v-model.number="formEntree.vehiculeId"
                               type="number"
                               required
                               min="1"
                               placeholder="ID du véhicule" />
                    </div>
                    <div class="field">
                        <label>Parking (ID)</label>
                        <input v-model.number="formEntree.parkingId"
                               type="number"
                               required
                               min="1"
                               placeholder="ID du parking" />
                    </div>
                    <div class="field">
                        <label>Abonnement (ID) <span class="optional">optionnel</span></label>
                        <input v-model.number="formEntree.abonnementId"
                               type="number"
                               min="1"
                               placeholder="ID de l'abonnement" />
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
    import { ref, reactive } from 'vue'

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