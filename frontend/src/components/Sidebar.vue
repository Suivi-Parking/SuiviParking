<template>
    <aside class="sidebar">
        <!-- Logo -->
        <div class="logo-area">
            <img src="../assets/images/logo.png" alt="ParkLine" />
            <span>ParkLine</span>
        </div>

        <!-- Navigation -->
        <nav class="nav-menu">
            <div v-for="section in sections" :key="section.title" class="nav-section">
                <p class="section-title">{{ section.title }}</p>
                <ul>
                    <li v-for="item in section.items" :key="item.key">
                        <button type="button"
                                class="nav-item"
                                :class="{ active: active === item.key }"
                                @click="$emit('navigate', item.key)">
                            <span v-html="item.icon" class="nav-icon" />
                            {{ item.label }}
                        </button>
                    </li>
                </ul>
            </div>
        </nav>

        <!-- Pied de page : profil + déconnexion -->
        <div class="sidebar-footer">
            <div class="user-profile">
              
               
                    <span class="text-sm text-slate-500 mr-3">{{ authStore.email}} ({{ authStore.role }})</span>
                  
               
            </div>
            <button @click="handleLogout" class="logout-btn">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
                    <polyline points="16 17 21 12 16 7" />
                    <line x1="21" y1="12" x2="9" y2="12" />
                </svg>
                Déconnexion
            </button>
        </div>
    </aside>
</template>

<script setup>
    import { authStore } from '../store/auth'

    defineProps({
        active: { type: String, default: 'entreprises' }
    })
    defineEmits(['navigate'])

    function handleLogout() {
        authStore.logout()
        window.location.href = '/login'
    }

    const icons = {
        dashboard: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="9" rx="1"/><rect x="14" y="3" width="7" height="5" rx="1"/><rect x="14" y="12" width="7" height="9" rx="1"/><rect x="3" y="16" width="7" height="5" rx="1"/></svg>`,
        building: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 21h18M5 21V7l4-4 4 4v14M9 21v-4"/><rect x="13" y="11" width="4" height="10"/><line x1="13" y1="7" x2="17" y2="7"/><line x1="13" y1="11" x2="17" y2="11"/></svg>`,
        users: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="9" cy="8" r="3"/><path d="M2 20c0-3.5 3-6 7-6s7 2.5 7 6"/><path d="M16 5.5c1.7.4 3 2 3 3.9 0 1.9-1.3 3.5-3 3.9"/><path d="M22 20c0-2.8-1.8-5-4-5.7"/></svg>`,
        car: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 17H3v-5l2-5h14l2 5v5h-2"/><circle cx="7" cy="17" r="2"/><circle cx="17" cy="17" r="2"/><path d="M5 12h14"/></svg>`,
        card: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="5" width="20" height="14" rx="2"/><line x1="2" y1="10" x2="22" y2="10"/></svg>`,
        invoice: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 2h9l5 5v15H6z"/><line x1="9" y1="13" x2="15" y2="13"/><line x1="9" y1="17" x2="15" y2="17"/></svg>`,
        euro: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 7a6 6 0 1 0 0 10"/><line x1="3" y1="10" x2="14" y2="10"/><line x1="3" y1="14" x2="12" y2="14"/></svg>`,
        bell: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 8a6 6 0 0 0-12 0c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>`
    }

    const sections = [
        {
            title: 'Principal',
            items: [{ key: 'dashboard', label: 'Tableau de bord', icon: icons.dashboard }]
        },
        {
            title: 'Gestion',
            items: [
                { key: 'employes', label: 'Employés', icon: icons.users },
                { key: 'vehicules', label: 'Véhicules', icon: icons.car },
                { key: 'Parking', label: 'Parking', icon: icons.car },
                { key: 'paiements', label: 'Paiements', icon: icons.euro },
                { key: 'entreprises', label: 'Entreprises clientes', icon: icons.building },
                { key: 'abonnements', label: 'Abonnements', icon: icons.card },
                { key: 'acces', label: 'Accès', icon: icons.card },
                { key: 'notifications', label: 'Notifications', icon: icons.bell }
            ]
        }
    ]
</script>

<style scoped>
    /* ===== SIDEBAR – Dégradé bleu clair ===== */
    .sidebar {
        position: sticky;
        top: 0;
        width: 280px;
        height: 100vh;
        background: linear-gradient(160deg, #ebf4ff 0%, #d4e4ff 100%);
        color: #1e293b;
        display: flex;
        flex-direction: column;
        border-right: 1px solid rgba(16, 106, 215, 0.08);
        box-shadow: 2px 0 16px rgba(16, 106, 215, 0.06);
        flex-shrink: 0;
        overflow: hidden;
    }

    /* ===== LOGO ===== */
    .logo-area {
        padding: 1.5rem 1.5rem 1rem;
        display: flex;
        align-items: center;
        gap: 0.75rem;
        border-bottom: 1px solid rgba(16, 106, 215, 0.08);
        flex-shrink: 0;
    }

        .logo-area img {
            width: 48px;
            height: auto;
            filter: drop-shadow(0 2px 4px rgba(16,106,215,0.1));
        }

        .logo-area span {
            font-size: 1.4rem;
            font-weight: 700;
            color: #0b1a2f;
            letter-spacing: -0.02em;
        }

    /* ===== NAVIGATION ===== */
    .nav-menu {
        flex: 1;
        padding: 1rem 0.6rem;
        overflow-y: auto;
    }

    .nav-section {
        margin-bottom: 1.5rem;
    }

    .section-title {
        font-size: 0.7rem;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.06em;
        color: #4b6a8b;
        padding: 0 0.6rem 0.4rem;
    }

    .nav-menu ul {
        list-style: none;
        padding: 0;
        margin: 0;
    }

    .nav-item {
        display: flex;
        align-items: center;
        gap: 0.7rem;
        width: 100%;
        padding: 0.6rem 0.8rem;
        border-radius: 10px;
        background: transparent;
        border: none;
        color: #2c405a;
        font-size: 0.9rem;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s;
    }

        .nav-item:hover {
            background: rgba(255, 255, 255, 0.5);
            color: #106ad7;
        }

        .nav-item.active {
            background: rgba(255, 255, 255, 0.7);
            color: #106ad7;
            font-weight: 600;
            box-shadow: inset 3px 0 0 #106ad7;
        }

    .nav-icon {
        width: 20px;
        height: 20px;
        display: flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;
    }

        .nav-icon svg {
            width: 100%;
            height: 100%;
            stroke: currentColor;
        }

    /* ===== FOOTER ===== */
    .sidebar-footer {
        padding: 1rem 0.8rem;
        border-top: 1px solid rgba(16, 106, 215, 0.08);
        flex-shrink: 0;
        background: rgba(255, 255, 255, 0.2);
        backdrop-filter: blur(4px);
    }

    .user-profile {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.5rem 0.5rem 0.8rem;
    }

    .avatar {
        width: 40px;
        height: 40px;
        border-radius: 12px;
        background: linear-gradient(135deg, #106ad7, #0b57c3);
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 600;
        font-size: 0.9rem;
        color: #fff;
        flex-shrink: 0;
        box-shadow: 0 2px 8px rgba(16,106,215,0.2);
    }

    .user-name {
        font-weight: 600;
        font-size: 0.9rem;
        color: #0b1a2f;
    }

    .user-role {
        font-size: 0.75rem;
        color: #4b6a8b;
    }

    .logout-btn {
        display: flex;
        align-items: center;
        gap: 0.6rem;
        width: 100%;
        padding: 0.5rem 0.8rem;
        background: rgba(255, 255, 255, 0.4);
        border: 1px solid rgba(16, 106, 215, 0.1);
        border-radius: 10px;
        color: #2c405a;
        font-size: 0.85rem;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s;
    }

        .logout-btn:hover {
            background: rgba(254, 226, 226, 0.6);
            border-color: #fca5a5;
            color: #b91c1c;
        }

        .logout-btn svg {
            width: 18px;
            height: 18px;
            stroke: currentColor;
        }
</style>