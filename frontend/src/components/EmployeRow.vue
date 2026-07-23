<template>
    <tr class="row-hover">
        <td class="cell">
            <div class="employee-info">
                <div class="avatar" :style="{ background: avatarColor }">
                    {{ initiales }}
                </div>
                <div>
                    <div class="font-semibold">{{ employe.nom }} {{ employe.prenom }}</div>
                    <div class="text-muted text-sm">ID-{{ employe.id }}</div>
                </div>
            </div>
        </td>
        <td class="cell">{{ employe.email }}</td>
        <td class="cell">{{ employe.telephone }}</td>
        <td class="cell">{{ employe.fonction }}</td>
        <td class="cell">
            <span class="badge" :class="statutBadge">{{ employe.statut === 'ACTIF' ? 'Actif' : 'Inactif' }}</span>
        </td>
        <td class="cell">{{ employe.vehicules?.length ?? 0 }} véhicule(s)</td>
        <td class="cell">
            <div class="action-group">
                <button class="btn-icon" @click="emit('view', employe)" title="Voir">👁️</button>
                <template v-if="authStore.can(PERMISSIONS.EMPLOYES_WRITE)">
                    <button class="btn-icon" @click="emit('edit', employe)" title="Modifier">✏️</button>
                    <button class="btn-icon btn-danger" @click="emit('delete', employe)" title="Supprimer">🗑️</button>
                </template>
            </div>
        </td>
    </tr>
</template>

<script setup>
    import { computed } from 'vue'
    import { authStore } from '../store/auth'
    import { PERMISSIONS } from '../services/permissions'

    const props = defineProps({ employe: { type: Object, required: true } })
    const emit = defineEmits(['view', 'edit', 'delete'])

    const initiales = computed(() => {
        const n = props.employe.nom?.[0] ?? ''
        const p = props.employe.prenom?.[0] ?? ''
        return (n + p).toUpperCase()
    })
    const avatarColor = computed(() => {
        const colors = ['#dbeafe', '#dcfce7', '#fef3c7', '#fce7f3', '#ede9fe']
        return colors[props.employe.id % colors.length]
    })
    const statutBadge = computed(() =>
        props.employe.statut === 'ACTIF' ? 'badge-success' : 'badge-danger'
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

    .employee-info {
        display: flex;
        align-items: center;
        gap: 0.7rem;
    }

    .avatar {
        width: 36px;
        height: 36px;
        border-radius: 10px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 700;
        font-size: 0.8rem;
        color: #1e293b;
        flex-shrink: 0;
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

    .badge-danger {
        background: #fee2e2;
        color: #b91c1c;
    }

    .action-group {
        display: flex;
        gap: 0.3rem;
    }

    .btn-icon {
        background: none;
        border: none;
        font-size: 1rem;
        padding: 0.2rem 0.4rem;
        border-radius: 8px;
        cursor: pointer;
        transition: background 0.2s;
    }

        .btn-icon:hover {
            background: #f1f5f9;
        }

    .btn-danger:hover {
        background: #fee2e2;
    }
</style>