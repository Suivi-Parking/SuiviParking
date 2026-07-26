<template>
    <tr class="row-hover">
        <td class="cell">
            <div class="vehicle-info">
                <div class="avatar">🚗</div>
                <div>
                    <div class="font-semibold">{{ vehicule.immatriculation }}</div>
                    <div class="text-muted text-sm">ID-{{ vehicule.id }}</div>
                </div>
            </div>
        </td>
        <td class="cell">
            <div class="font-medium">{{ vehicule.marque }}</div>
            <div class="text-muted text-sm">{{ vehicule.modele }}</div>
        </td>
        <td class="cell">{{ vehicule.couleur }}</td>
        <td class="cell">{{ vehicule.type }}</td>
        <td class="cell">{{ vehicule.employeNom ?? 'N/A' }}</td>
        <td class="cell">
            <span class="badge" :class="statutBadge">{{ vehicule.statut === 'ACTIF' ? 'Actif' : 'Inactif' }}</span>
        </td>
        <td class="cell">
            <div class="action-group">
                <button class="btn-icon" @click="emit('view', vehicule)" title="Voir">👁️</button>
                <template v-if="authStore.can(PERMISSIONS.VEHICULES_WRITE)">
                    <button class="btn-icon" @click="emit('edit', vehicule)" title="Modifier">✏️</button>
                    <button class="btn-icon btn-danger" @click="emit('delete', vehicule)" title="Supprimer">🗑️</button>
                </template>
            </div>
        </td>
    </tr>
</template>

<script setup>
    import { computed } from 'vue'
    import { authStore } from '../store/auth'
    import { PERMISSIONS } from '../services/permissions'

    const props = defineProps({ vehicule: { type: Object, required: true } })
    const emit = defineEmits(['view', 'edit', 'delete'])

    const statutBadge = computed(() =>
        props.vehicule.statut === 'ACTIF' ? 'badge-success' : 'badge-danger'
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

    .vehicle-info {
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
        font-size: 1.2rem;
        background: #dbeafe;
        flex-shrink: 0;
    }

    .font-semibold {
        font-weight: 600;
    }

    .font-medium {
        font-weight: 500;
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