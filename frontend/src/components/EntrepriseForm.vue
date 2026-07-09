<template>
  <div class="modal-backdrop" style="position:fixed;inset:0;display:flex;align-items:center;justify-content:center;z-index:60">
    <div class="modal" style="width:600px;background:white;border-radius:10px;box-shadow:0 10px 30px rgba(0,0,0,0.2);overflow:hidden">
      <header style="padding:16px;border-bottom:1px solid #eef2f6;display:flex;justify-content:space-between;align-items:center">
        <h3 style="margin:0">{{ mode === 'create' ? 'Nouvelle entreprise' : 'Modifier entreprise' }}</h3>
        <button type="button" @click="$emit('cancel')" style="border:none;background:transparent;font-size:18px;cursor:pointer">✖</button>
      </header>

      <form @submit.prevent="submit" style="padding:16px;">
        <div style="display:flex;gap:12px;flex-wrap:wrap">
          <div style="flex:1;min-width:260px">
            <label class="block text-sm">Nom *</label>
            <input v-model="form.Nom" type="text" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
            <div v-if="errors.Nom" class="text-sm" style="color:#dc2626">{{ errors.Nom }}</div>
          </div>

          <div style="flex:1;min-width:260px">
            <label class="block text-sm">Matricule Fiscal *</label>
            <input v-model="form.MatriculeFiscal" type="text" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
            <div v-if="errors.MatriculeFiscal" class="text-sm" style="color:#dc2626">{{ errors.MatriculeFiscal }}</div>
          </div>

          <div style="flex:1;min-width:260px">
            <label class="block text-sm">Secteur</label>
            <input v-model="form.Secteur" type="text" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
          </div>

          <div style="flex:1;min-width:260px">
            <label class="block text-sm">Téléphone</label>
            <input v-model="form.Telephone" type="text" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
            <div v-if="errors.Telephone" class="text-sm" style="color:#dc2626">{{ errors.Telephone }}</div>
          </div>

          <div style="flex:1;min-width:260px">
            <label class="block text-sm">Email</label>
            <input v-model="form.Email" type="email" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
            <div v-if="errors.Email" class="text-sm" style="color:#dc2626">{{ errors.Email }}</div>
          </div>

          <div style="flex-basis:100%">
            <label class="block text-sm">Adresse</label>
            <input v-model="form.Adresse" type="text" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
          </div>
        </div>

        <div style="display:flex;justify-content:flex-end;gap:8px;margin-top:16px">
          <button type="button" @click="$emit('cancel')" style="padding:8px 12px;border-radius:6px;border:1px solid #cbd5e1;background:#fff">Annuler</button>
          <button type="submit" class="primary">{{ mode === 'create' ? 'Créer' : 'Enregistrer' }}</button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { reactive, toRefs, watch } from 'vue'
const props = defineProps({ mode: { type: String, default: 'create' }, entreprise: { type: Object, default: null } })
const emit = defineEmits(['save','cancel'])

const form = reactive({ Nom: '', Adresse: '', Telephone: '', Email: '', MatriculeFiscal: '', Secteur: '' })
const errors = reactive({})

watch(()=> props.entreprise, (v) => {
  if(v){
    form.Nom = v.nom ?? v.Nom ?? ''
    form.Adresse = v.adresse ?? v.Adresse ?? ''
    form.Telephone = v.telephone ?? v.Telephone ?? ''
    form.Email = v.email ?? v.Email ?? ''
    form.MatriculeFiscal = v.matriculeFiscal ?? v.MatriculeFiscal ?? ''
    form.Secteur = v.secteur ?? v.Secteur ?? ''
  } else {
    form.Nom = ''
    form.Adresse = ''
    form.Telephone = ''
    form.Email = ''
    form.MatriculeFiscal = ''
    form.Secteur = ''
  }
}, { immediate: true })

function validate(){
  errors.Nom = !form.Nom ? 'Le nom est requis' : ''
  errors.MatriculeFiscal = !form.MatriculeFiscal ? 'Le matricule fiscal est requis' : ''
  errors.Email = form.Email && !/^\S+@\S+\.\S+$/.test(form.Email) ? 'Email invalide' : ''
  errors.Telephone = form.Telephone && !/^\+?[0-9 \-]{6,20}$/.test(form.Telephone) ? 'Téléphone invalide' : ''
  return !errors.Nom && !errors.MatriculeFiscal && !errors.Email && !errors.Telephone
}

function submit(){
  if(!validate()) return
  emit('save', { ...form })
}
</script>

<style scoped>
.modal-backdrop{background:rgba(15,23,42,0.4)}
</style>
