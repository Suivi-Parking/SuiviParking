<template>
  <div>
    <h1 class="text-2xl font-semibold mb-1">Entreprises clientes</h1>
    <p class="text-sm text-muted mb-4">{{ totalCount }} entreprises enregistrées sur la plateforme</p>

    <div class="flex items-center gap-3 mb-4">
      <button v-for="f in filters" :key="f" type="button" @click="filter = f" :class="filter === f ? 'primary' : ''">
        {{ labels[f] }} <span class="badge">{{ counts[f] ?? 0 }}</span>
      </button>

      <input v-model="search" placeholder="Rechercher une entreprise..." style="margin-left:auto;padding:8px;border:1px solid #cbd5e1;border-radius:6px;" />
    </div>

    <div class="bg-panel rounded-lg shadow-sm overflow-hidden border">
      <table class="w-full">
        <thead class="text-xs uppercase text-muted bg-[var(--bg)]">
          <tr>
            <th class="p-3 text-left">Nom de l'entreprise</th>
            <th class="p-3">Secteur</th>
            <th class="p-3">Nb d'employés</th>
            <th class="p-3">Statut d'abonnement</th>
            <th class="p-3">Date de création</th>
            <th class="p-3">Actions</th>
          </tr>
        </thead>
        <tbody>
          <template v-for="e in paged" :key="e.id">
            <EntrepriseRow @view="onView" @edit="openEdit" @delete="onDelete" :entreprise="e" />
          </template>
        </tbody>
      </table>
    </div>

    <EntrepriseForm v-if="showForm" :mode="formMode" :entreprise="selectedEntreprise" @save="onFormSave" @cancel="() => { showForm = false }" />

    <div class="mt-4" style="display:flex;justify-content:flex-end;align-items:center;gap:8px;color:#6b7280;font-size:14px;margin-top:12px">
      <button type="button" @click="prevPage" :disabled="page<=1">Préc</button>
      <div>Page {{ page }} / {{ totalPages }}</div>
      <button type="button" @click="nextPage" :disabled="page>=totalPages">Suiv</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import EntrepriseRow from './EntrepriseRow.vue'
import EntrepriseForm from './EntrepriseForm.vue'

const data = ref([])
const search = ref('')
const filter = ref('Tous')
const page = ref(1)
const pageSize = ref(10)

const filters = ['Tous','Actif','EnAttente','Expire']
const labels = { 'Tous':'Tous', 'Actif':'Actif', 'EnAttente':'En attente', 'Expire':'Expiré' }
const counts = {
  'Tous': 0, 'Actif':0, 'EnAttente':0, 'Expire':0
}

async function loadData(){
  try {
    const res = await fetch('/api/entreprises')
    const json = await res.json()
    // si votre API retourne l'objet EntrepriseListeResponseDto
    if (json && json.items) {
      data.value = json.items
      counts.Tous = json.totalCount ?? data.value.length
    } else {
      data.value = json
      counts.Tous = data.value.length
    }
  } catch (e) {
    // fallback sample data
    data.value = [
      { id:1, nom:'Groupe Renault Industries', matriculeFiscal:'GRI-001', secteur:'Automobile', nombreEmployes:4820, statutAbonnement:'Actif', dateCreation:'2023-01-12' },
      { id:2, nom:'BNP Paribas Asset Management', matriculeFiscal:'BNP-002', secteur:'Finance', nombreEmployes:1340, statutAbonnement:'Actif', dateCreation:'2023-03-03' },
      { id:3, nom:'Veolia Environnement', matriculeFiscal:'VE-003', secteur:'Energie', nombreEmployes:890, statutAbonnement:'EnAttente', dateCreation:'2023-04-17' },
    ]
    counts.Tous = data.value.length
  }

  // compute counts
  counts.Actif = data.value.filter(x=>x.statutAbonnement==='Actif').length
  counts.EnAttente = data.value.filter(x=>x.statutAbonnement==='EnAttente').length
  counts.Expire = data.value.filter(x=>x.statutAbonnement==='Expire').length
}

onMounted(()=> loadData())

// handlers for row actions
async function onDelete(entreprise){
  if(!confirm(`Supprimer l'entreprise "${entreprise.nom}" ?`)) return
  try{
    const res = await fetch(`/api/entreprises/${entreprise.id}`, { method: 'DELETE' })
    if(!res.ok){
      let body = ''
      try { body = await res.text() } catch(e){ body = '' }
      throw new Error(`Suppression échouée (${res.status}) ${body}`)
    }
    await loadData()
  }catch(err){
    console.error(err)
    alert(err.message || 'La suppression a échoué')
  }
}

async function onEdit(entreprise){
  const newName = prompt('Modifier le nom', entreprise.nom)
  if(!newName) return
  try{
    // Compose update DTO using available fields
    const dto = {
      Nom: newName,
      Adresse: entreprise.adresse ?? '',
      Telephone: entreprise.telephone ?? '',
      Email: entreprise.email ?? '',
      MatriculeFiscal: entreprise.matriculeFiscal ?? '',
      Secteur: entreprise.secteur ?? ''
    }
    const res = await fetch(`/api/entreprises/${entreprise.id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(dto) })
    if(!res.ok){
      let body = ''
      try{ body = await res.text() }catch(e){ body = '' }
      throw new Error(`Mise à jour échouée (${res.status}) ${body}`)
    }
    await loadData()
  }catch(err){
    console.error(err)
    alert(err.message || 'La mise à jour a échoué')
  }
}

function onView(entreprise){
  // show details in modal in read-only mode (reuse form but do not allow save)
  alert(JSON.stringify(entreprise, null, 2))
}

// create flow triggered by Header via window event
async function openCreate(){
  formMode.value = 'create'
  selectedEntreprise.value = null
  showForm.value = true
}

onMounted(()=> window.addEventListener('create-entreprise', openCreate))
onUnmounted(()=> window.removeEventListener('create-entreprise', openCreate))

// form modal state (use refs so template reactivity works)
const showForm = ref(false)
const formMode = ref('create')
const selectedEntreprise = ref(null)

async function onFormSave(payload){
  try{
    if(formMode.value === 'create'){
      const res = await fetch('/api/entreprises', { method: 'POST', headers: { 'Content-Type':'application/json' }, body: JSON.stringify(payload) })
      if(!res.ok) throw new Error('Erreur création')
    } else {
      const id = selectedEntreprise.value?.id
      const res = await fetch(`/api/entreprises/${id}`, { method: 'PUT', headers: { 'Content-Type':'application/json' }, body: JSON.stringify(payload) })
      if(!res.ok) throw new Error('Erreur update')
    }
    showForm.value = false
    await loadData()
  }catch(err){ console.error(err); alert("L'opération a échoué") }
}

function openEdit(entreprise){ formMode.value = 'edit'; selectedEntreprise.value = entreprise; showForm.value = true }



const filtered = computed(()=> {
  let list = data.value || []
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(x => (x.nom||'').toLowerCase().includes(q) || (x.matriculeFiscal||'').toLowerCase().includes(q))
  }
  if (filter.value !== 'Tous') list = list.filter(x=> x.statutAbonnement === filter.value)
  return list
})

const totalCount = computed(()=> filtered.value.length)
const totalPages = computed(()=> Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

const paged = computed(()=> {
  const start = (page.value-1)*pageSize.value
  return filtered.value.slice(start, start + pageSize.value)
})

function prevPage(){ if (page.value>1) page.value-- }
function nextPage(){ if (page.value<totalPages.value) page.value++ }

</script>
