<template>
    <tr class="row-hover" :class="statutClasse">
        <td class="cell">#{{ acces.id }}</td>
        <td class="cell font-medium">{{ acces.vehiculeImmatriculation }}</td>
        <td class="cell">{{ acces.employeNom }}</td>
        <td class="cell">{{ formaterDate(acces.dateHeureEntree) }}</td>
        <td class="cell">{{ acces.dateHeureSortie ? formaterDate(acces.dateHeureSortie) : '—' }}</td>
        <td class="cell">{{ acces.dureeFormatee || '—' }}</td>
        <td class="cell">
            <span class="badge" :class="badgeClass">{{ libelleStatut }}</span>
        </td>
        <td class="cell">{{ acces.motifRefus || '—' }}</td>
        <td class="cell">
            <button v-if="acces.statut === 'EN_COURS'"
                    class="btn-sortie"
                    @click="$emit('enregistrer-sortie', acces.id)">
                Sortie
            </button>
        </td>
    </tr>
</template>

<script setup>
    import { computed } from 'vue'

    const props = defineProps({ acces: { type: Object, required: true } })
    defineEmits(['enregistrer-sortie'])

    const libelles = {
        EN_COURS: 'En cours',
        SORTI: 'Sorti',
        REFUSE: 'Refusé',
    }
    const libelleStatut = computed(() => libelles[props.acces.statut] || props.acces.statut)

    const badgeClass = {
        EN_COURS: 'badge-en-cours',
        SORTI: 'badge-sorti',
        REFUSE: 'badge-refuse',
    }[props.acces.statut] || 'badge-neutral'

    const statutClasse = {
        EN_COURS: 'row-en-cours',
        SORTI: 'row-sorti',
        REFUSE: 'row-refuse',
    }[props.acces.statut] || ''

    function formaterDate(valeur) {
        if (!valeur) return '—'
        return new Date(valeur).toLocaleString('fr-FR')
    }
</script>

<style scoped>
    .row-hover:hover {
        background: #f8fafc;
    }

    .cell {
        padding: 0.7rem 0.6rem;
        border-bottom: 1px solid #f1f5f9;
        vertical-align: middle;
    }

    .font-medium {
        font-weight: 500;
    }

    .badge {
        display: inline-block;
        padding: 0.2rem 0.8rem;
        border-radius: 50px;
        font-size: 0.75rem;
        font-weight: 600;
    }

    .badge-en-cours {
        background: #dbeafe;
        color: #1d4ed8;
    }

    .badge-sorti {
        background: #e2e8f0;
        color: #475569;
    }

    .badge-refuse {
        background: #fee2e2;
        color: #b91c1c;
    }

    .badge-neutral {
        background: #f1f5f9;
        color: #64748b;
    }

    .btn-sortie {
        padding: 0.2rem 0.8rem;
        border-radius: 30px;
        border: none;
        background: #106ad7;
        color: #fff;
        font-size: 0.75rem;
        font-weight: 500;
        cursor: pointer;
        transition: background 0.2s;
    }

        .btn-sortie:hover {
            background: #0b57c3;
        }

    /* Indicateur visuel pour les lignes en cours */
    .row-en-cours td:first-child {
        border-left: 3px solid #106ad7;
    }
</style>