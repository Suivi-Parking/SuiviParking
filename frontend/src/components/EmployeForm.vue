
<template>
  <div style="position:fixed;inset:0;background:rgba(0,0,0,0.4);display:flex;align-items:center;justify-content:center;z-index:999;">
    <div class="bg-panel" style="width:500px;border-radius:12px;padding:28px;box-shadow:0 8px 32px rgba(0,0,0,0.18);max-height:90vh;overflow-y:auto;">

      <h2 class="font-medium" style="font-size:18px;margin-bottom:20px;">
        {{ mode === 'create' ? '+ Nouvel employé' : 'Modifier l\'employé' }}
      </h2>

      <div style="display:grid;grid-template-columns:1fr 1fr;gap:14px;">

        <div>
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Nom *</label>
          <input v-model="form.nom" placeholder="Nom"
            :style="errors.nom ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.nom" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.nom }}</p>
        </div>

        <div>
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Prénom *</label>
          <input v-model="form.prenom" placeholder="Prénom"
            :style="errors.prenom ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.prenom" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.prenom }}</p>
        </div>

        <div style="grid-column:span 2;">
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Email *</label>
          <input v-model="form.email" type="email" placeholder="email@exemple.com"
            :style="errors.email ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.email" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.email }}</p>
        </div>

        <div>
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Téléphone *</label>
          <input v-model="form.telephone" placeholder="55000000"
            :style="errors.telephone ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.telephone" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.telephone }}</p>
        </div>

        <div>
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Fonction *</label>
          <input v-model="form.fonction" placeholder="Développeur, Manager..."
            :style="errors.fonction ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.fonction" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.fonction }}</p>
        </div>

        <div>
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Entreprise ID *</label>
          <input v-model.number="form.entrepriseId" type="number" placeholder="1"
            :style="errors.entrepriseId ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.entrepriseId" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.entrepriseId }}</p>
        </div>

        <div v-if="mode === 'edit'">
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Statut</label>
          <select v-model="form.statut"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;">
            <option value="ACTIF">ACTIF</option>
            <option value="INACTIF">INACTIF</option>
          </select>
        </div>

      </div>

      <div style="display:flex;justify-content:flex-end;gap:10px;margin-top:24px;">
        <button type="button" @click="$emit('cancel')">Annuler</button>
        <button type="button" class="primary" @click="onSave">
          {{ mode === 'create' ? 'Créer' : 'Enregistrer' }}
        </button>
      </div>

    </div>
  </div>
</template>
<<script setup>
import { ref, watch, computed } from 'vue'

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

  if (!form.value.nom?.trim())
    errors.value.nom = '⚠️ Le nom est obligatoire'

  if (!form.value.prenom?.trim())
    errors.value.prenom = '⚠️ Le prénom est obligatoire'

  if (!form.value.email?.trim())
    errors.value.email = '⚠️ L\'email est obligatoire'
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.value.email))
    errors.value.email = '⚠️ Format email invalide (ex: nom@gmail.com)'

  if (!form.value.telephone?.trim())
    errors.value.telephone = '⚠️ Le téléphone est obligatoire'
  else if (!/^\d{8}$/.test(form.value.telephone))
    errors.value.telephone = '⚠️ Téléphone invalide (8 chiffres)'

  if (!form.value.fonction?.trim())
    errors.value.fonction = '⚠️ La fonction est obligatoire'

  if (!form.value.entrepriseId || form.value.entrepriseId <= 0)
    errors.value.entrepriseId = '⚠️ L\'ID entreprise est invalide'

  return Object.keys(errors.value).length === 0
}

async function onSave() {
  if (!validate()) return
  emit('save', { ...form.value })
}
</script>