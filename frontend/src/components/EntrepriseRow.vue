<template>
    <tr class="row-hover">
        <td class="cell">
            <div class="company-info">
                <div class="avatar" :style="{ background: avatarColor }">
                    {{ initials }}
                </div>
                <div>
                    <div class="font-semibold">{{ entreprise.nom }}</div>
                    <div class="text-muted text-sm">{{ entreprise.matriculeFiscal }}</div>
                </div>
            </div>
        </td>
        <td class="cell">
            <span class="sector-badge">{{ entreprise.secteur || '—' }}</span>
        </td>
        <td class="cell text-center">{{ entreprise.nombreEmployes ?? 0 }}</td>
        <td class="cell">
            <span class="badge" :class="statusBadge">{{ statutText }}</span>
        </td>
        <td class="cell">{{ formattedDate }}</td>
        <td class="cell">
            <div class="action-group">
                <button class="btn-icon" @click="emit('view', entreprise)" title="Voir">👁️</button>
                <template v-if="authStore.can(PERMISSIONS.ENTREPRISES_WRITE)">
                    <button class="btn-icon" @click="emit('edit', entreprise)" title="Modifier">✏️</button>
                    <button class="btn-icon btn-danger" @click="emit('delete', entreprise)" title="Supprimer">🗑️</button>
                </template>
            </div>
        </td>
    </tr>
</template>

<script setup>
    import { computed } from 'vue'
    import { authStore } from '../store/auth'
    import { PERMISSIONS } from '../services/permissions'

    const props = defineProps({ entreprise: { type: Object, required: true } })
    const emit = defineEmits(['view', 'edit', 'delete'])

    const initials = computed(() => {
        const nom = props.entreprise.nom || ''
        const parts = nom.split(' ')
        return parts.slice(0, 2).map(p => p[0]).join('').toUpperCase()
    })
    const avatarColor = computed(() => {
        const colors = ['#dbeafe', '#dcfce7', '#fef3c7', '#fce7f3', '#ede9fe']
        return colors[props.entreprise.id % colors.length]
    })
    const statutText = computed(() => {
        const statut = props.entreprise.statutAbonnement || 'EnAttente'
        return statut === 'Actif' ? 'Actif' : statut === 'EnAttente' ? 'En attente' : 'Expiré'
    })
    const statusBadge = computed(() => {
        const statut = props.entreprise.statutAbonnement || 'EnAttente'
        if (statut === 'Actif') return 'badge-success'
        if (statut === 'EnAttente') return 'badge-warning'
        return 'badge-danger'
    })
    const formattedDate = computed(() => {
        const date = props.entreprise.dateCreation || Date.now()
        return new Date(date).toLocaleDateString('fr-FR')
    })
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

    .company-info {
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

    .text-center {
        text-align: center;
    }

    .sector-badge {
        display: inline-block;
        padding: 0.2rem 0.8rem;
        border-radius: 50px;
        background: #f1f5f9;
        color: #475569;
        font-size: 0.8rem;
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

    .badge-warning {
        background: #fef3c7;
        color: #b45309;
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