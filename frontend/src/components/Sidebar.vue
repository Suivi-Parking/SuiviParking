<template>
<aside class="w-64 h-screen border-r flex flex-col" style="background:#e8f4fd;">
   <div class="p-5 font-bold text-primary text-lg flex items-center gap-1">
    <img src="../assets/images/logo.png" alt="ParkLine Logo" 
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

        <div class="p-4 border-t">
            <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-full bg-primary/10 text-primary flex items-center justify-center font-medium">
                    GW
                </div>
                <div>
                    <div class="font-medium text-sm">Ghouma Wafa</div>
                    <div class="text-xs text-gray-400">Administrateur</div>
                </div>
            </div>
        </div>
    </aside>
</template>

<script setup>
defineProps({
    active: { type: String, default: 'entreprises' }
})
defineEmits(['navigate'])

const icons = {
    dashboard: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="9" rx="1"/><rect x="14" y="3" width="7" height="5" rx="1"/><rect x="14" y="12" width="7" height="9" rx="1"/><rect x="3" y="16" width="7" height="5" rx="1"/></svg>',
    building: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="4" y="2" width="16" height="20" rx="1"/><line x1="9" y1="7" x2="9" y2="7.01"/><line x1="15" y1="7" x2="15" y2="7.01"/><line x1="9" y1="12" x2="9" y2="12.01"/><line x1="15" y1="12" x2="15" y2="12.01"/><line x1="9" y1="17" x2="15" y2="17"/></svg>',
    users: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="9" cy="8" r="3"/><path d="M2 20c0-3.5 3-6 7-6s7 2.5 7 6"/><path d="M16 5.5c1.7.4 3 2 3 3.9 0 1.9-1.3 3.5-3 3.9"/><path d="M22 20c0-2.8-1.8-5-4-5.7"/></svg>',
    car: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 17H3v-5l2-5h14l2 5v5h-2"/><circle cx="7" cy="17" r="2"/><circle cx="17" cy="17" r="2"/><path d="M5 12h14"/></svg>',
    card: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="5" width="20" height="14" rx="2"/><line x1="2" y1="10" x2="22" y2="10"/></svg>',
    invoice: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2h9l5 5v15H6z"/><line x1="9" y1="13" x2="15" y2="13"/><line x1="9" y1="17" x2="15" y2="17"/></svg>',
  
    euro: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 7a6 6 0 1 0 0 10"/><line x1="3" y1="10" x2="14" y2="10"/><line x1="3" y1="14" x2="12" y2="14"/></svg>'
}
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
            { key: 'paiements', label: 'Paiements', icon: icons.euro },
            { key: 'facturation', label: 'Facturation', icon: icons.invoice }
        ]
    }
]
</script>