<template>
    <div class="calib-wrap">
        <div class="calib-header">
            <div class="calib-header-row">
                <h2>Calibration des zones — Parking #{{ parkingId }}</h2>
                <button class="btn-close" @click="$emit('close')" title="Fermer">✕</button>
            </div>
            <p class="hint">
                Choisis la zone physique concernée, clique sur l'image pour placer ses points
                (minimum 3), puis clique sur "Terminer la zone". Recommence pour chaque zone.
            </p>
        </div>

        <div v-if="erreur" class="alert error">{{ erreur }}</div>
        <div v-if="messageSucces" class="alert success">{{ messageSucces }}</div>

        <div v-if="chargementImage" class="loading">Chargement de l'image...</div>

        <div v-else class="canvas-area" ref="canvasArea">
            <img ref="imageRef"
                 :src="imageObjectUrl"
                 @load="onImageLoad"
                 class="calib-image"
                 alt="Image du parking" />
            <canvas ref="canvasRef" class="calib-canvas" @click="onCanvasClick"></canvas>
        </div>

        <div class="controls">
            <div class="field">
                <label>Zone physique à calibrer</label>
                <select v-model="zoneIdEnCours">
                    <option value="" disabled>-- Choisir une zone --</option>
                    <option v-for="z in zonesDisponibles" :key="z.id" :value="z.id">
                        {{ z.nom }} ({{ z.nombrePlaces }} places)
                    </option>
                </select>
            </div>
            <button class="btn btn-primary" :disabled="!peutTerminerZone" @click="terminerZone">
                Terminer la zone ({{ pointsEnCours.length }} point{{ pointsEnCours.length > 1 ? 's' : '' }})
            </button>
            <button class="btn btn-ghost" :disabled="pointsEnCours.length === 0" @click="annulerZoneEnCours">
                Annuler le tracé en cours
            </button>
        </div>

        <p v-if="zonesDisponibles.length === 0 && !chargementImage" class="hint warn">
            Aucune zone disponible à calibrer — soit toutes les zones du parking sont déjà calibrées,
            soit ce parking n'a pas encore de zone créée.
        </p>

        <div class="zones-liste" v-if="zones.length">
            <h3>Zones calibrées ({{ zones.length }})</h3>
            <ul>
                <li v-for="(zone, index) in zones" :key="zone.zoneId">
                    <span class="pastille" :style="{ background: couleurZone(index) }"></span>
                    <strong>{{ nomZone(zone.zoneId) }}</strong>
                    <span class="muted">{{ zone.points.length }} points</span>
                    <button class="btn-icon" @click="supprimerZone(index)" title="Supprimer">🗑</button>
                </li>
            </ul>
        </div>

        <div class="actions-finales">
            <button class="btn btn-primary" :disabled="enregistrement" @click="enregistrerCalibration">
                {{ enregistrement ? 'Enregistrement...' : 'Enregistrer la calibration' }}
            </button>
        </div>
    </div>
</template>

<script setup>
    import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
    import api from '../services/api'

    const props = defineProps({
        parkingId: { type: [Number, String], required: true }
    })
    defineEmits(['close'])

    const chargementImage = ref(true)
    const imageObjectUrl = ref('')
    const imageRef = ref(null)
    const canvasRef = ref(null)
    const canvasArea = ref(null)

    const toutesLesZonesDuParking = ref([])  // Zones physiques du parking (id, nom, nombrePlaces)
    const zones = ref([])                     // calibrations déjà terminées dans cette session { zoneId, points }
    const pointsEnCours = ref([])
    const zoneIdEnCours = ref('')

    const erreur = ref('')
    const messageSucces = ref('')
    const enregistrement = ref(false)

    const PALETTE = ['#1B5FAE', '#2E9E5B', '#D64545', '#E0A32E', '#8B5CF6', '#0EA5E9', '#EC4899']
    function couleurZone(index) {
        return PALETTE[index % PALETTE.length]
    }
    function nomZone(zoneId) {
        return toutesLesZonesDuParking.value.find(z => z.id === zoneId)?.nom || `Zone #${zoneId}`
    }

    // Zones physiques pas encore calibrées, pour éviter de calibrer deux fois la même
    const zonesDisponibles = computed(() =>
        toutesLesZonesDuParking.value.filter(z => !zones.value.some(zc => zc.zoneId === z.id))
    )

    const peutTerminerZone = computed(() => pointsEnCours.value.length >= 3 && !!zoneIdEnCours.value)

    // ===== Chargement des zones physiques du parking (dropdown) =====
    async function chargerZonesDuParking() {
        try {
            const response = await api.get(`/Parkings/${props.parkingId}`)
            toutesLesZonesDuParking.value = (response.data.zones || []).map(z => ({
                id: z.id, nom: z.nom, nombrePlaces: z.nombrePlaces
            }))
        } catch (e) {
            erreur.value = "Impossible de charger les zones de ce parking."
        }
    }

    // ===== Chargement de l'image (via le backend, en blob) =====
    async function chargerImage() {
        chargementImage.value = true
        try {
            const response = await api.get(`/Calibration/image/${props.parkingId}`, { responseType: 'blob' })
            imageObjectUrl.value = URL.createObjectURL(response.data)
        } catch (e) {
            erreur.value = e?.response?.data?.message || "Impossible de charger l'image du parking."
        } finally {
            chargementImage.value = false
        }
    }

    // ===== Chargement de la calibration existante =====
    async function chargerCalibrationExistante() {
        try {
            const response = await api.get(`/Calibration/${props.parkingId}`)
            zones.value = response.data.map(z => ({
                zoneId: z.zoneId,
                points: [],              // rempli en pixels une fois l'image chargee (onImageLoad)
                pointsNormalises: z.points
            }))
        } catch (e) {
            console.error('Impossible de charger la calibration existante', e)
        }
    }

    function onImageLoad() {
        const img = imageRef.value
        const canvas = canvasRef.value
        if (!img || !canvas) return

        canvas.width = img.clientWidth
        canvas.height = img.clientHeight

        zones.value = zones.value.map(z => ({
            zoneId: z.zoneId,
            points: (z.pointsNormalises || z.points || []).map(p => ({
                x: p.x * canvas.width,
                y: p.y * canvas.height
            }))
        }))

        dessiner()
    }

    function onResize() {
        if (imageRef.value && canvasRef.value) {
            canvasRef.value.width = imageRef.value.clientWidth
            canvasRef.value.height = imageRef.value.clientHeight
            dessiner()
        }
    }

    function onCanvasClick(event) {
        const rect = canvasRef.value.getBoundingClientRect()
        pointsEnCours.value.push({ x: event.clientX - rect.left, y: event.clientY - rect.top })
        dessiner()
    }

    function terminerZone() {
        if (!peutTerminerZone.value) return
        zones.value.push({ zoneId: Number(zoneIdEnCours.value), points: [...pointsEnCours.value] })
        pointsEnCours.value = []
        zoneIdEnCours.value = ''
        dessiner()
    }

    function annulerZoneEnCours() {
        pointsEnCours.value = []
        dessiner()
    }

    function supprimerZone(index) {
        zones.value.splice(index, 1)
        dessiner()
    }

    function dessiner() {
        const canvas = canvasRef.value
        if (!canvas) return
        const ctx = canvas.getContext('2d')
        ctx.clearRect(0, 0, canvas.width, canvas.height)

        zones.value.forEach((zone, index) => {
            dessinerPolygone(ctx, zone.points, couleurZone(index), true)
        })

        if (pointsEnCours.value.length > 0) {
            dessinerPolygone(ctx, pointsEnCours.value, '#111827', false)
        }
    }

    function dessinerPolygone(ctx, points, couleur, ferme) {
        if (points.length === 0) return

        ctx.beginPath()
        ctx.moveTo(points[0].x, points[0].y)
        for (let i = 1; i < points.length; i++) ctx.lineTo(points[i].x, points[i].y)
        if (ferme) ctx.closePath()

        ctx.strokeStyle = couleur
        ctx.lineWidth = 2
        ctx.setLineDash(ferme ? [] : [6, 4])
        ctx.stroke()

        if (ferme) {
            ctx.fillStyle = couleur + '33'
            ctx.fill()
        }

        points.forEach(p => {
            ctx.beginPath()
            ctx.arc(p.x, p.y, 4, 0, Math.PI * 2)
            ctx.fillStyle = couleur
            ctx.fill()
        })
    }

    async function enregistrerCalibration() {
        if (zones.value.length === 0) {
            erreur.value = "Ajoute au moins une zone avant d'enregistrer."
            return
        }

        const canvas = canvasRef.value
        const payload = zones.value.map(z => ({
            zoneId: z.zoneId,
            points: z.points.map(p => ({ x: p.x / canvas.width, y: p.y / canvas.height }))
        }))

        enregistrement.value = true
        erreur.value = ''
        messageSucces.value = ''
        try {
            await api.put(`/Calibration/${props.parkingId}`, payload)
            messageSucces.value = 'Calibration enregistrée avec succès.'
            setTimeout(() => { messageSucces.value = '' }, 3000)
        } catch (e) {
            erreur.value = e?.response?.data?.message || "Erreur lors de l'enregistrement de la calibration."
        } finally {
            enregistrement.value = false
        }
    }

    onMounted(async () => {
        window.addEventListener('resize', onResize)
        await chargerZonesDuParking()
        await chargerCalibrationExistante()
        await chargerImage()
    })

    onBeforeUnmount(() => {
        window.removeEventListener('resize', onResize)
        if (imageObjectUrl.value) URL.revokeObjectURL(imageObjectUrl.value)
    })
</script>

<style scoped>
    .calib-wrap {
        max-width: 900px;
        margin: 0 auto;
        padding: 1.5rem;
    }

    .calib-header h2 {
        margin: 0 0 0.25rem;
    }

    .calib-header-row {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 1rem;
    }

    .btn-close {
        background: #f1f5f9;
        border: none;
        border-radius: 8px;
        width: 32px;
        height: 32px;
        cursor: pointer;
        font-size: 1rem;
        color: #475569;
        flex-shrink: 0;
    }

        .btn-close:hover {
            background: #fee2e2;
            color: #dc2626;
        }

    .hint {
        color: #64748b;
        font-size: 0.9rem;
        margin: 0 0 1rem;
    }

        .hint.warn {
            color: #b45309;
            background: #fffbeb;
            padding: 0.5rem 0.75rem;
            border-radius: 6px;
        }

    .alert {
        padding: 0.6rem 1rem;
        border-radius: 8px;
        margin-bottom: 1rem;
        font-size: 0.9rem;
    }

        .alert.error {
            background: #fef2f2;
            color: #dc2626;
        }

        .alert.success {
            background: #f0fdf4;
            color: #16a34a;
        }

    .loading {
        padding: 3rem;
        text-align: center;
        color: #94a3b8;
    }

    .canvas-area {
        position: relative;
        display: inline-block;
        max-width: 100%;
    }

    .calib-image {
        display: block;
        max-width: 100%;
        border-radius: 8px;
        user-select: none;
    }

    .calib-canvas {
        position: absolute;
        top: 0;
        left: 0;
        cursor: crosshair;
    }

    .controls {
        display: flex;
        align-items: flex-end;
        gap: 0.75rem;
        margin-top: 1rem;
        flex-wrap: wrap;
    }

    .field {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
    }

        .field label {
            font-size: 0.8rem;
            color: #64748b;
        }

        .field select {
            padding: 0.5rem 0.7rem;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            min-width: 240px;
        }

    .btn {
        padding: 0.55rem 1rem;
        border-radius: 8px;
        border: none;
        cursor: pointer;
        font-weight: 600;
        font-size: 0.9rem;
    }

        .btn:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

    .btn-primary {
        background: #1B5FAE;
        color: #fff;
    }

    .btn-ghost {
        background: #f1f5f9;
        color: #334155;
    }

    .zones-liste {
        margin-top: 1.5rem;
    }

        .zones-liste ul {
            list-style: none;
            padding: 0;
            margin: 0.5rem 0 0;
            display: flex;
            flex-direction: column;
            gap: 0.4rem;
        }

        .zones-liste li {
            display: flex;
            align-items: center;
            gap: 0.6rem;
            padding: 0.5rem 0.75rem;
            background: #f8fafc;
            border-radius: 8px;
        }

    .pastille {
        width: 10px;
        height: 10px;
        border-radius: 50%;
        flex-shrink: 0;
    }

    .muted {
        color: #94a3b8;
        font-size: 0.85rem;
        margin-left: auto;
    }

    .btn-icon {
        background: none;
        border: none;
        cursor: pointer;
        font-size: 0.9rem;
    }

    .actions-finales {
        margin-top: 1.5rem;
    }
</style>