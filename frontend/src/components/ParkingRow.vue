<template>
    <tr class="row-hover">
        <td class="cell">
            <div>
                <div class="font-semibold">{{ parking.nom }}</div>
                <div class="text-muted text-sm">{{ parking.adresse }}</div>
            </div>
        </td>
        <td class="cell">
            <span class="code-badge">{{ parking.code }}</span>
        </td>
        <td class="cell">
            <span class="badge" :class="statutBadge">{{ parking.statut }}</span>
        </td>
        <td class="cell text-center">{{ parking.nombreZones }}</td>
        <td class="cell" style="min-width: 140px;">
            <div class="occupation-wrapper">
                <div class="progress-bar-bg">
                    <div class="progress-bar-fill"
                         :class="barColor"
                         :style="{ width: Math.min(parking.tauxOccupation, 100) + '%' }"></div>
                </div>
                <span class="occupation-text">
                    {{ parking.placesOccupees }}/{{ parking.capaciteMax }}
                </span>
            </div>
        </td>
        <td class="cell">
            <div class="action-group">
                <button class="btn-action btn-detail" @click="$emit('voir')">Détail</button>
                <template v-if="authStore.can(PERMISSIONS.PARKINGS_WRITE)">
                    <button class="btn-action btn-edit" @click="$emit('modifier')">Modifier</button>
                    <button class="btn-action btn-danger" @click="$emit('supprimer')">Archiver</button>
                </template>
            </div>
        </td>
    </tr>
</template>

<script setup>
    import { computed } from 'vue'
    import { authStore } from '../store/auth'
    import { PERMISSIONS } from '../services/permissions'

    const props = defineProps({ parking: { type: Object, required: true } })
    defineEmits(['voir', 'modifier', 'supprimer'])

    const barColor = computed(() => {
        const taux = props.parking.tauxOccupation
        if (taux > 90) return 'bg-danger'
        if (taux >= 70) return 'bg-warning'
        return 'bg-success'
    })

    const statutBadge = computed(() =>
        props.parking.statut === 'Actif' ? 'badge-success' : 'badge-inactif'
    )
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

    .font-semibold {
        font-weight: 600;
    }

    .text-muted {
        color: #64748b;
    }

    .text-sm {
        font-size: 0.75rem;
    }

    .text-center {
        text-align: center;
    }

    .code-badge {
        display: inline-block;
        padding: 0.2rem 0.6rem;
        border-radius: 4px;
        background: #f1f5f9;
        font-family: monospace;
        font-size: 0.8rem;
        color: #1e293b;
    }

    .badge {
        display: inline-block;
        padding: 0.2rem 0.8rem;
        border-radius: 50px;
        font-size: 0.75rem;
        font-weight: 600;
    }

    .badge-success {
        background: #dcfce7;
        color: #15803d;
    }

    .badge-inactif {
        background: #e2e8f0;
        color: #475569;
    }

    .occupation-wrapper {
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }

    .progress-bar-bg {
        flex: 1;
        height: 6px;
        background: #e2e8f0;
        border-radius: 999px;
        overflow: hidden;
    }

    .progress-bar-fill {
        height: 100%;
        border-radius: 999px;
        transition: width 0.3s ease;
    }

    .bg-success {
        background: #22c55e;
    }

    .bg-warning {
        background: #eab308;
    }

    .bg-danger {
        background: #ef4444;
    }

    .occupation-text {
        font-size: 0.75rem;
        color: #64748b;
        white-space: nowrap;
    }

    .action-group {
        display: flex;
        flex-wrap: wrap;
        gap: 0.3rem;
    }

    .btn-action {
        padding: 0.2rem 0.8rem;
        border-radius: 30px;
        border: 1px solid transparent;
        font-size: 0.75rem;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s;
    }

    .btn-detail {
        background: #eef2f6;
        color: #1e293b;
        border-color: #e2e8f0;
    }

        .btn-detail:hover {
            background: #e2e8f0;
        }

    .btn-edit {
        background: #dbeafe;
        color: #1d4ed8;
        border-color: #93c5fd;
    }

        .btn-edit:hover {
            background: #bfdbfe;
        }

    .btn-danger {
        background: #fee2e2;
        color: #b91c1c;
        border-color: #fca5a5;
    }

        .btn-danger:hover {
            background: #fecaca;
        }
</style>