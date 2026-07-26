<template>
    <div class="login-container">
        <!-- Canvas Three.js en arrière-plan -->
        <div ref="threeContainer" class="three-bg"></div>

        <!-- Carte de connexion -->
        <div class="login-card">
            <div class="login-header">
                <img src="../assets/images/logo.png" alt="ParkLine" class="logo" />
                <h1>ParkLine</h1>
                <p>Espace administrateur</p>
            </div>

            <form @submit.prevent="handleLogin" class="login-form">
                <div class="form-group">
                    <label>Email</label>
                    <input v-model="email" type="email" required placeholder="exemple@parkline.com" />
                </div>
                <div class="form-group">
                    <label>Mot de passe</label>
                    <input v-model="password" type="password" required placeholder="••••••••" />
                </div>

                <p v-if="errorMessage" class="error-msg">{{ errorMessage }}</p>

                <button type="submit" :disabled="isLoading" class="btn-primary">
                    {{ isLoading ? 'Connexion...' : 'Se connecter' }}
                </button>
            </form>
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted, onUnmounted } from 'vue'
    import { authStore } from '../store/auth'
    import * as THREE from 'three'

    const email = ref('')
    const password = ref('')
    const errorMessage = ref('')
    const isLoading = ref(false)

    // Référence au conteneur Three.js
    const threeContainer = ref(null)
    let scene, camera, renderer, cubes = [], particles

    // Initialisation de la scène Three.js
    function initThree() {
        const container = threeContainer.value
        const width = container.clientWidth
        const height = container.clientHeight

        scene = new THREE.Scene()
        scene.background = new THREE.Color(0x0f172a) // fond sombre

        camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000)
        camera.position.set(5, 5, 10)
        camera.lookAt(0, 0, 0)

        renderer = new THREE.WebGLRenderer({ antialias: true })
        renderer.setSize(width, height)
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2))
        container.appendChild(renderer.domElement)

        // Lumières
        const ambientLight = new THREE.AmbientLight(0x404060)
        scene.add(ambientLight)
        const dirLight = new THREE.DirectionalLight(0xffffff, 1)
        dirLight.position.set(1, 2, 1)
        scene.add(dirLight)

        // Cubes flottants
        const colors = [0x106ad7, 0x2b6fb6, 0x4a8fd4, 0x7bb3e6]
        for (let i = 0; i < 20; i++) {
            const size = 0.3 + Math.random() * 0.6
            const geometry = new THREE.BoxGeometry(size, size, size)
            const material = new THREE.MeshStandardMaterial({
                color: colors[Math.floor(Math.random() * colors.length)],
                roughness: 0.3,
                metalness: 0.1,
                transparent: true,
                opacity: 0.6 + Math.random() * 0.4,
            })
            const cube = new THREE.Mesh(geometry, material)
            cube.position.set(
                (Math.random() - 0.5) * 12,
                (Math.random() - 0.5) * 8,
                (Math.random() - 0.5) * 6
            )
            cube.rotation.set(Math.random() * Math.PI, Math.random() * Math.PI, 0)
            scene.add(cube)
            cubes.push(cube)
        }

        // Particules (système de points)
        const particleCount = 800
        const positions = new Float32Array(particleCount * 3)
        for (let i = 0; i < particleCount * 3; i++) {
            positions[i] = (Math.random() - 0.5) * 30
        }
        const particleGeometry = new THREE.BufferGeometry()
        particleGeometry.setAttribute('position', new THREE.BufferAttribute(positions, 3))
        const particleMaterial = new THREE.PointsMaterial({
            color: 0x88ccff,
            size: 0.05,
            transparent: true,
            opacity: 0.6,
        })
        particles = new THREE.Points(particleGeometry, particleMaterial)
        scene.add(particles)
    }

    // Animation loop
    function animate() {
        requestAnimationFrame(animate)

        // Rotation des cubes
        cubes.forEach((cube, i) => {
            cube.rotation.x += 0.005 * (i % 2 === 0 ? 1 : -1)
            cube.rotation.y += 0.01 * (i % 3 === 0 ? 1 : -1)
            cube.position.y += Math.sin(Date.now() * 0.001 + i) * 0.001
        })

        // Rotation des particules
        if (particles) {
            particles.rotation.y += 0.0005
        }

        renderer.render(scene, camera)
    }

    // Gestion du redimensionnement
    function onResize() {
        const container = threeContainer.value
        const width = container.clientWidth
        const height = container.clientHeight
        camera.aspect = width / height
        camera.updateProjectionMatrix()
        renderer.setSize(width, height)
    }

    // Méthode de connexion
    async function handleLogin() {
        errorMessage.value = ''
        isLoading.value = true
        try {
            await authStore.login(email.value, password.value)
        } catch (error) {
            errorMessage.value = 'Email ou mot de passe incorrect.'
        } finally {
            isLoading.value = false
        }
    }

    onMounted(() => {
        initThree()
        animate()
        window.addEventListener('resize', onResize)
    })

    onUnmounted(() => {
        window.removeEventListener('resize', onResize)
        // Nettoyage Three.js
        if (renderer) {
            renderer.dispose()
            const container = threeContainer.value
            if (container && renderer.domElement) {
                container.removeChild(renderer.domElement)
            }
        }
    })
</script>

<style scoped>
    .login-container {
        position: relative;
        width: 100vw;
        height: 100vh;
        overflow: hidden;
        background: #0f172a;
    }

    .three-bg {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        z-index: 0;
    }

    .login-card {
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        width: 420px;
        max-width: 90%;
        padding: 2.5rem 2rem;
        background: rgba(255, 255, 255, 0.08);
        backdrop-filter: blur(16px);
        -webkit-backdrop-filter: blur(16px);
        border-radius: 32px;
        border: 1px solid rgba(255, 255, 255, 0.15);
        box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.6);
        z-index: 1;
        color: #fff;
        transition: all 0.3s ease;
    }

    .login-header {
        text-align: center;
        margin-bottom: 2rem;
    }

    .logo {
        width: 80px;
        height: auto;
        margin-bottom: 0.5rem;
        filter: drop-shadow(0 4px 6px rgba(0,0,0,0.3));
    }

    .login-header h1 {
        font-size: 2rem;
        font-weight: 700;
        letter-spacing: -0.025em;
        background: linear-gradient(135deg, #88ccff, #106ad7);
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
        margin: 0;
    }

    .login-header p {
        margin-top: 0.25rem;
        font-size: 0.9rem;
        opacity: 0.7;
        font-weight: 300;
    }

    .login-form .form-group {
        margin-bottom: 1.2rem;
    }

    .login-form label {
        display: block;
        font-size: 0.85rem;
        font-weight: 500;
        margin-bottom: 0.3rem;
        color: rgba(255,255,255,0.8);
    }

    .login-form input {
        width: 100%;
        padding: 0.75rem 1rem;
        background: rgba(255, 255, 255, 0.1);
        border: 1px solid rgba(255, 255, 255, 0.2);
        border-radius: 12px;
        color: #fff;
        font-size: 0.95rem;
        transition: all 0.2s;
    }

        .login-form input::placeholder {
            color: rgba(255,255,255,0.4);
        }

        .login-form input:focus {
            outline: none;
            border-color: #106ad7;
            background: rgba(255, 255, 255, 0.15);
            box-shadow: 0 0 0 4px rgba(16, 106, 215, 0.2);
        }

    .error-msg {
        color: #f87171;
        font-size: 0.85rem;
        margin-top: -0.5rem;
        margin-bottom: 1rem;
    }

    .btn-primary {
        width: 100%;
        padding: 0.8rem;
        background: linear-gradient(135deg, #106ad7, #0b57c3);
        border: none;
        border-radius: 12px;
        font-weight: 600;
        font-size: 1rem;
        color: #fff;
        cursor: pointer;
        transition: all 0.2s;
        box-shadow: 0 4px 12px rgba(16, 106, 215, 0.4);
    }

        .btn-primary:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 8px 20px rgba(16, 106, 215, 0.5);
        }

        .btn-primary:disabled {
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }
</style>