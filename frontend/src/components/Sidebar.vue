<template>
    <aside class="w-64 h-screen border-r flex flex-col" style="background:#e8f4fd;">
        <div class="p-5 font-bold text-primary text-lg flex items-center gap-1">
            <img src="../assets/images/logo.png"
                 alt="ParkLine Logo"
                 style="width:120px;height:110px;object-fit:contain;mix-blend-mode:multiply;flex-shrink:0;vertical-align:middle;" />
            <span style="font-size:18px;font-weight:700;color:#106ad7;line-height:120px;"></span>
        </div>

        <nav class="px-3 flex-1 overflow-y-auto">
            <div v-for="section in sections" :key="section.title" class="mb-6">
                <p class="px-3 mb-2 text-[11px] font-semibold tracking-wide text-gray-400 uppercase">
                    {{ section.title }}
                </p>
                <ul class="space-y-1">
                    <li v-for="item in section.items" :key="item.key">
                        <button type="button"
                                class="w-full flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-left transition-colors"
                                :class="active === item.key
                                    ? 'bg-primary/10 text-primary font-medium border-l-2 border-primary'
                                    : 'text-gray-600 hover:bg-gray-50 border-l-2 border-transparent'"
                                @click="$emit('navigate', item.key)">
                            <span v-html="item.icon" class="w-4 h-4 flex-shrink-0" />
                            {{ item.label }}
                        </button>
                    </li>
                </ul>
            </div>
        </nav>

        <!-- Bloc utilisateur dynamique avec authStore.email et authStore.role -->
        <div class="p-4 border-t">
            <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-full bg-primary/10 text-primary flex items-center justify-center font-medium">
                    {{ userInitials }}
                </div>
                <div>
                    <div class="font-medium text-sm">{{ userFullName }}</div>
                    <div class="text-xs text-gray-400">{{ userRole }}</div>
                </div>
            </div>
        </div>
    </aside>
</template>

<script setup>
import { computed } from 'vue'
import { authStore } from '../store/auth'   // Assurez-vous que le chemin est correct

defineProps({
    active: {
        type: String,
        default: 'entreprises'
    }
})

defineEmits(['navigate'])

// --- Icônes SVG (inchangées) ---
const icons = {
    dashboard: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="9" rx="1"/><rect x="14" y="3" width="7" height="5" rx="1"/><rect x="14" y="12" width="7" height="9" rx="1"/><rect x="3" y="16" width="7" height="5" rx="1"/></svg>',
    building: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="4" y="2" width="16" height="20" rx="1"/><line x1="9" y1="7" x2="9" y2="7.01"/><line x1="15" y1="7" x2="15" y2="7.01"/><line x1="9" y1="12" x2="9" y2="12.01"/><line x1="15" y1="12" x2="15" y2="12.01"/><line x1="9" y1="17" x2="15" y2="17"/></svg>',
    users: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="9" cy="8" r="3"/><path d="M2 20c0-3.5 3-6 7-6s7 2.5 7 6"/><path d="M16 5.5c1.7.4 3 2 3 3.9 0 1.9-1.3 3.5-3 3.9"/><path d="M22 20c0-2.8-1.8-5-4-5.7"/></svg>',
    car: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 17H3v-5l2-5h14l2 5v5h-2"/><circle cx="7" cy="17" r="2"/><circle cx="17" cy="17" r="2"/><path d="M5 12h14"/></svg>',
    card: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="5" width="20" height="14" rx="2"/><line x1="2" y1="10" x2="22" y2="10"/></svg>',
    invoice: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2h9l5 5v15H6z"/><line x1="9" y1="13" x2="15" y2="13"/><line x1="9" y1="17" x2="15" y2="17"/></svg>',
    lock: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>'
}

// --- Structure des menus (inchangée) ---
const sections = [
    {
        title: 'Principal',
        items: [
            { key: 'dashboard', label: 'Tableau de bord', icon: icons.dashboard },
            { key: 'entreprises', label: 'Entreprises clientes', icon: icons.building },
            { key: 'abonnements', label: 'Abonnements', icon: icons.card }
        ]
    },
    {
        title: 'Gestion',
        items: [
            { key: 'employes', label: 'Employés', icon: icons.users },
            { key: 'vehicules', label: 'Véhicules', icon: icons.car },
            { key: 'Parking', label: 'Parking', icon: icons.car },
            { key: 'acces', label: 'Accès', icon: icons.lock },
            { key: 'facturation', label: 'Facturation', icon: icons.invoice }
        ]
    }
]

// --- Données utilisateur depuis authStore ---
// authStore expose directement email et role (pas un objet user)
const userFullName = computed(() => authStore.email || 'Invité')
const userRole = computed(() => authStore.role || '')
const userInitials = computed(() => {
    const name = userFullName.value
    if (!name || name === 'Invité') return '??'
    // Prend les deux premières lettres de l'email (partie avant le @)
    const localPart = name.split('@')[0] || name
    return localPart.substring(0, 2).toUpperCase()
})
</script>