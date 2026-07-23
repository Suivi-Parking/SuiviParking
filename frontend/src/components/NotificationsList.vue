<template>
    <div class="p-6">
        <div v-if="!authStore.can(PERMISSIONS.NOTIFICATIONS_READ)" class="text-danger">
            Vous n'avez pas la permission de consulter les notifications.
        </div>

        <template v-else>
            <div class="flex flex-wrap items-center gap-3 mb-4">
                <select v-model="filtres.type"
                        @change="rafraichir(1)"
                        class="border border-slate-200 rounded-lg px-3 py-2 text-sm">
                    <option value="">Tous les types</option>
                    <option v-for="t in typesNotification" :key="t.value" :value="t.value">
                        {{ t.label }}
                    </option>
                </select>

                <input v-model.number="filtres.entrepriseId"
                       @change="rafraichir(1)"
                       type="number"
                       min="1"
                       placeholder="ID entreprise (optionnel)"
                       class="border border-slate-200 rounded-lg px-3 py-2 text-sm w-56" />

                <button type="button"
                        @click="rafraichir(1)"
                        class="primary text-sm">
                    Filtrer
                </button>
            </div>

            <p v-if="chargement" class="text-muted text-sm">Chargement...</p>
            <p v-else-if="erreur" class="text-danger text-sm">{{ erreur }}</p>

            <div v-else class="bg-panel border border-slate-200 rounded-xl overflow-hidden">
                <table class="w-full text-sm">
                    <thead>
                        <tr class="text-left text-xs uppercase text-muted border-b border-slate-200">
                            <th class="px-4 py-3">Type</th>
                            <th class="px-4 py-3">Canal</th>
                            <th class="px-4 py-3">Statut</th>
                            <th class="px-4 py-3">Destinataire</th>
                            <th class="px-4 py-3">Sujet</th>
                            <th class="px-4 py-3">Entreprise</th>
                            <th class="px-4 py-3">Créée le</th>
                            <th class="px-4 py-3">Envoyée le</th>
                            <th class="px-4 py-3"></th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="n in items"
                            :key="n.id"
                            class="border-b border-slate-100 last:border-0">
                            <td class="px-4 py-3">{{ libelleType(n.type) }}</td>
                            <td class="px-4 py-3">{{ n.canal }}</td>
                            <td class="px-4 py-3">
                                <span :class="badgeClasse(n.statut)" class="px-2 py-1 rounded-md text-xs font-medium">
                                    {{ n.statut }}
                                </span>
                            </td>
                            <td class="px-4 py-3">{{ n.destinataire }}</td>
                            <td class="px-4 py-3 max-w-xs truncate" :title="n.sujet">{{ n.sujet }}</td>
                            <td class="px-4 py-3">{{ n.entrepriseNom || '—' }}</td>
                            <td class="px-4 py-3">{{ formaterDate(n.dateCreation) }}</td>
                            <td class="px-4 py-3">{{ n.dateEnvoi ? formaterDate(n.dateEnvoi) : '—' }}</td>
                            <td class="px-4 py-3">
                                <button v-if="authStore.can(PERMISSIONS.NOTIFICATIONS_WRITE)"
                                        type="button"
                                        @click="renvoyer(n.id)"
                                        :disabled="renvoiEnCours === n.id"
                                        class="text-primary text-xs font-medium hover:underline disabled:opacity-50">
                                    {{ renvoiEnCours === n.id ? 'Envoi...' : 'Renvoyer' }}
                                </button>
                            </td>
                        </tr>
                        <tr v-if="items.length === 0">
                            <td colspan="9" class="px-4 py-6 text-center text-muted">
                                Aucune notification trouvée.
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class="flex items-center gap-4 mt-4" v-if="totalPages > 1">
                <button :disabled="page <= 1"
                        @click="rafraichir(page - 1)"
                        class="px-3 py-1 border border-slate-200 rounded-lg text-sm disabled:opacity-40">
                    Précédent
                </button>
                <span class="text-sm text-muted">Page {{ page }} / {{ totalPages }}</span>
                <button :disabled="page >= totalPages"
                        @click="rafraichir(page + 1)"
                        class="px-3 py-1 border border-slate-200 rounded-lg text-sm disabled:opacity-40">
                    Suivant
                </button>
            </div>
        </template>
    </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import api from '../services/api'
import { authStore } from '../store/auth'
import { PERMISSIONS } from '../services/permissions'

const items = ref([])
const page = ref(1)
const pageSize = ref(20)
const totalPages = ref(1)
const chargement = ref(false)
const erreur = ref('')
const renvoiEnCours = ref(null)

const filtres = reactive({
  type: '',
  entrepriseId: null,
})

const typesNotification = [
  { value: 'RenouvellementAbonnement', label: 'Renouvellement abonnement' },
  { value: 'RappelPaiement', label: 'Rappel de paiement' },
  { value: 'AccesRefuse', label: 'Accès refusé' },
  { value: 'SaturationParking', label: 'Saturation parking' },
  { value: 'MiseAJourInfos', label: 'Mise à jour infos' },
]

function libelleType(type) {
  return typesNotification.find(t => t.value === type)?.label || type
}

function badgeClasse(statut) {
  if (statut === 'Envoyee') return 'bg-green-100 text-green-700'
  if (statut === 'Echouee') return 'bg-red-100 text-red-700'
  return 'bg-amber-100 text-amber-700' // EnAttente
}

function formaterDate(valeur) {
  if (!valeur) return '—'
  return new Date(valeur).toLocaleString('fr-FR')
}

async function rafraichir(nouvellePage = page.value) {
  if (!authStore.can(PERMISSIONS.NOTIFICATIONS_READ)) return

  chargement.value = true
  erreur.value = ''
  page.value = nouvellePage

  try {
    const params = { page: page.value, pageSize: pageSize.value }
    if (filtres.type) params.type = filtres.type
    if (filtres.entrepriseId) params.entrepriseId = filtres.entrepriseId

    const reponse = await api.get('/Notifications', { params })

    items.value = reponse.data.items
    totalPages.value = reponse.data.totalPages
  } catch (e) {
    erreur.value = e.response?.data?.message || 'Erreur lors du chargement des notifications.'
  } finally {
    chargement.value = false
  }
}

async function renvoyer(id) {
  renvoiEnCours.value = id
  try {
    await api.post(`/Notifications/${id}/renvoyer`)
    await rafraichir()
  } catch (e) {
    erreur.value = e.response?.data?.message || 'Erreur lors du renvoi.'
  } finally {
    renvoiEnCours.value = null
  }
}

onMounted(() => rafraichir())
</script>