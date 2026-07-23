<template>
    <div class="modal-backdrop" @click.self="$emit('cancel')">
        <div class="modal-card">
            <header class="modal-header">
                <h3>{{ mode === 'create' ? 'Nouvel employé' : 'Modifier l\'employé' }}</h3>
                <button class="btn-close" @click="$emit('cancel')">✕</button>
            </header>

            <form @submit.prevent="onSave" class="modal-body">
                <div class="grid-2">
                    <div class="field">
                        <label>Nom *</label>
                        <input v-model="form.nom" placeholder="Nom" :class="{ error: errors.nom }" />
                        <span v-if="errors.nom" class="error-msg">{{ errors.nom }}</span>
                    </div>
                    <div class="field">
                        <label>Prénom *</label>
                        <input v-model="form.prenom" placeholder="Prénom" :class="{ error: errors.prenom }" />
                        <span v-if="errors.prenom" class="error-msg">{{ errors.prenom }}</span>
                    </div>
                    <div class="field span-2">
                        <label>Email *</label>
                        <input v-model="form.email" type="email" placeholder="email@exemple.com" :class="{ error: errors.email }" />
                        <span v-if="errors.email" class="error-msg">{{ errors.email }}</span>
                    </div>
                    <div class="field">
                        <label>Téléphone *</label>
                        <input v-model="form.telephone" placeholder="55000000" :class="{ error: errors.telephone }" />
                        <span v-if="errors.telephone" class="error-msg">{{ errors.telephone }}</span>
                    </div>
                    <div class="field">
                        <label>Fonction *</label>
                        <input v-model="form.fonction" placeholder="Développeur, Manager..." :class="{ error: errors.fonction }" />
                        <span v-if="errors.fonction" class="error-msg">{{ errors.fonction }}</span>
                    </div>
                    <div class="field">
                        <label>Entreprise ID *</label>
                        <input v-model.number="form.entrepriseId" type="number" placeholder="1" :class="{ error: errors.entrepriseId }" />
                        <span v-if="errors.entrepriseId" class="error-msg">{{ errors.entrepriseId }}</span>
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
    import { ref, watch } from 'vue'

    const props = defineProps({
        mode: { type: String, default: 'create' },
        employe: { type: Object, default: null }
    })
    const emit = defineEmits(['save', 'cancel'])
    const errors = ref({})

    const form = ref({
        nom: '',
        prenom: '',
        email: '',
        telephone: '',
        fonction: '',
        entrepriseId: 1,
        statut: 'ACTIF'
    })

    watch(() => props.employe, (val) => {
        if (val) {
            form.value = {
                id: val.id,
                nom: val.nom,
                prenom: val.prenom,
                email: val.email,
                telephone: val.telephone,
                fonction: val.fonction,
                entrepriseId: val.entrepriseId,
                statut: val.statut
            }
        } else {
            form.value = {
                nom: '', prenom: '', email: '',
                telephone: '', fonction: '',
                entrepriseId: 1, statut: 'ACTIF'
            }
        }
        errors.value = {}
    }, { immediate: true })

    function validate() {
        errors.value = {}
        if (!form.value.nom?.trim()) errors.value.nom = '⚠️ Le nom est obligatoire'
        if (!form.value.prenom?.trim()) errors.value.prenom = '⚠️ Le prénom est obligatoire'
        if (!form.value.email?.trim()) errors.value.email = '⚠️ L\'email est obligatoire'
        else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.value.email))
            errors.value.email = '⚠️ Format email invalide'
        if (!form.value.telephone?.trim()) errors.value.telephone = '⚠️ Le téléphone est obligatoire'
        else if (!/^\d{8}$/.test(form.value.telephone))
            errors.value.telephone = '⚠️ Téléphone invalide (8 chiffres)'
        if (!form.value.fonction?.trim()) errors.value.fonction = '⚠️ La fonction est obligatoire'
        if (!form.value.entrepriseId || form.value.entrepriseId <= 0)
            errors.value.entrepriseId = '⚠️ L\'ID entreprise est invalide'
        return Object.keys(errors.value).length === 0
    }

    async function onSave() {
        if (!validate()) return
        emit('save', { ...form.value })
    }
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
        width: 600px;
        max-width: 94%;
        background: #fff;
        border-radius: 20px;
        box-shadow: 0 20px 60px rgba(0, 0, 0, 0.25);
        overflow: hidden;
    }

    .modal-header {
        padding: 1.2rem 1.5rem;
        border-bottom: 1px solid #eef2f6;
        display: flex;
        justify-content: space-between;
        align-items: center;
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