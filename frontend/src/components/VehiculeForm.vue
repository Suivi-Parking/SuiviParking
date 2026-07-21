<template>
  <div style="position:fixed;inset:0;background:rgba(0,0,0,0.4);display:flex;align-items:center;justify-content:center;z-index:999;">
    <div class="bg-panel" style="width:500px;border-radius:12px;padding:28px;box-shadow:0 8px 32px rgba(0,0,0,0.18);max-height:90vh;overflow-y:auto;">

      <h2 class="font-medium" style="font-size:18px;margin-bottom:20px;">
        {{ mode === 'create' ? '+ Nouveau véhicule' : 'Modifier le véhicule' }}
      </h2>

      <div style="display:grid;grid-template-columns:1fr 1fr;gap:14px;">

        <div style="grid-column:span 2;">
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Immatriculation *</label>
          <input v-model="form.immatriculation" placeholder="TUN-1234"
            :style="errors.immatriculation ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.immatriculation" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.immatriculation }}</p>
        </div>

        <div>
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Marque *</label>
          <input v-model="form.marque" placeholder="Toyota, Peugeot..."
            :style="errors.marque ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.marque" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.marque }}</p>
        </div>

        <div>
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Modèle *</label>
          <input v-model="form.modele" placeholder="Corolla, 208..."
            :style="errors.modele ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.modele" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.modele }}</p>
        </div>

        <div>
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Couleur *</label>
          <input v-model="form.couleur" placeholder="Blanc, Noir..."
            :style="errors.couleur ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.couleur" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.couleur }}</p>
        </div>

        <div>
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Type *</label>
          <select v-model="form.type"
            :style="errors.type ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;">
            <option value="">Sélectionner...</option>
            <option value="Berline">Berline</option>
            <option value="Citadine">Citadine</option>
            <option value="SUV">SUV</option>
            <option value="Break">Break</option>
            <option value="Utilitaire">Utilitaire</option>
          </select>
          <p v-if="errors.type" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.type }}</p>
        </div>

        <div>
          <label class="text-muted" style="font-size:12px;display:block;margin-bottom:4px;">Employé ID *</label>
          <input v-model.number="form.employeId" type="number" placeholder="1"
            :style="errors.employeId ? 'border-color:#ef4444;' : ''"
            style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px;box-sizing:border-box;" />
          <p v-if="errors.employeId" style="color:#ef4444;font-size:11px;margin-top:3px;">{{ errors.employeId }}</p>
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

<script setup>
import { ref, watch } from 'vue'

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
  } else {
    form.value = {
      immatriculation: '', marque: '', modele: '',
      couleur: '', type: '', employeId: 1, statut: 'ACTIF'
    }
  }
  errors.value = {}
}, { immediate: true })

function validate() {
  errors.value = {}

  if (!form.value.immatriculation?.trim())
    errors.value.immatriculation = '⚠️ L\'immatriculation est obligatoire'
  else if (!/^[A-Z]{2,3}-\d{3,4}$/i.test(form.value.immatriculation))
    errors.value.immatriculation = '⚠️ Format invalide (ex: TUN-1234)'

  if (!form.value.marque?.trim())
    errors.value.marque = '⚠️ La marque est obligatoire'

  if (!form.value.modele?.trim())
    errors.value.modele = '⚠️ Le modèle est obligatoire'

  if (!form.value.couleur?.trim())
    errors.value.couleur = '⚠️ La couleur est obligatoire'

  if (!form.value.type)
    errors.value.type = '⚠️ Le type est obligatoire'

  if (!form.value.employeId || form.value.employeId <= 0)
    errors.value.employeId = '⚠️ L\'ID employé est invalide'

  return Object.keys(errors.value).length === 0
}

async function onSave() {
  if (!validate()) return
  emit('save', { ...form.value })
}
</script>