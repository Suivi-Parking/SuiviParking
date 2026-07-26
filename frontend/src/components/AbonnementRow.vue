<script setup>
    import { authStore } from '../store/auth'
    import { PERMISSIONS } from '../services/permissions'

    defineProps({
        abonnement: { type: Object, required: true }
    })
    defineEmits(['renouveler', 'suspendre', 'resilier'])

    const badgeClasses = {
        Actif: 'bg-success/10 text-success',
        Suspendu: 'bg-warning/10 text-warning',
        Resilie: 'bg-danger/10 text-danger',
        Expire: 'bg-danger/10 text-danger'
    }
    const dotClasses = {
        Actif: 'bg-success',
        Suspendu: 'bg-warning',
        Resilie: 'bg-danger',
        Expire: 'bg-danger'
    }
    const labels = {
        Actif: 'Actif',
        Suspendu: 'Suspendu',
        Resilie: 'Résilié',
        Expire: 'Expiré'
    }

    function formatDate(d) {
        return new Date(d).toLocaleDateString('fr-FR')
    }
    function formatMontant(m) {
        return new Intl.NumberFormat('fr-TN', { minimumFractionDigits: 0, maximumFractionDigits: 2 }).format(m) + ' DT'
    }
</script>

<template>
    <tr class="border-b last:border-0 hover:bg-gray-50">
        <td class="p-3">
            <div class="font-semibold">{{ abonnement.entrepriseNom }}</div>
        </td>
        <td class="p-3 text-gray-600">{{ abonnement.typeAbonnement }}</td>
        <td class="p-3 text-gray-600">{{ formatDate(abonnement.dateDebut) }}</td>
        <td class="p-3 text-gray-600">{{ formatDate(abonnement.dateFin) }}</td>
        <td class="p-3 font-medium">{{ formatMontant(abonnement.montantMensuel) }}</td>
        <td class="p-3">
            <span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium"
                  :class="badgeClasses[abonnement.statut]">
                <span class="w-1.5 h-1.5 rounded-full" :class="dotClasses[abonnement.statut]" />
                {{ labels[abonnement.statut] ?? abonnement.statut }}
            </span>
            <div v-if="abonnement.raison" class="text-xs text-gray-400 mt-1">{{ abonnement.raison }}</div>
        </td>
        <td class="p-3">
            <div class="flex gap-2" v-if="authStore.can(PERMISSIONS.ABONNEMENTS_WRITE)">
                <button v-if="abonnement.statut === 'Expire'"
                        class="px-3 py-1.5 rounded-lg border text-xs font-medium hover:bg-gray-50"
                        @click="$emit('renouveler', abonnement)">
                    Renouveler
                </button>
                <button v-if="abonnement.statut === 'Actif'"
                        class="px-3 py-1.5 rounded-lg border text-xs font-medium hover:bg-gray-50"
                        @click="$emit('suspendre', abonnement)">
                    Suspendre
                </button>
                <button v-if="abonnement.statut === 'Actif' || abonnement.statut === 'Suspendu'"
                        class="px-3 py-1.5 rounded-lg border border-danger/30 text-danger text-xs font-medium hover:bg-danger/5"
                        @click="$emit('resilier', abonnement)">
                    Résilier
                </button>
                <span v-if="abonnement.statut === 'Resilie'" class="text-gray-300 text-xs">—</span>
            </div>
            <span v-else class="text-gray-300 text-xs">—</span>
        </td>
    </tr>
</template>
