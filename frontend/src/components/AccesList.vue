<template>
  <div class="acces-list">
    <h2>Gestion des accès</h2>

    <AccesForm @acces-cree="() => rafraichir()" @acces-modifie="() => rafraichir()" />

    <div class="filtres">
      <select v-model="filtres.statut" @change="rafraichir(1)">
        <option value="">Tous les statuts</option>
        <option value="EN_COURS">En cours</option>
        <option value="SORTI">Sorti</option>
        <option value="REFUSE">Refusé</option>
      </select>
    </div>

    <p v-if="chargement">Chargement...</p>
    <p v-else-if="erreur" class="erreur">{{ erreur }}</p>

    <table v-else>
      <thead>
        <tr>
          <th>ID</th>
          <th>Immatriculation</th>
          <th>Employé</th>
          <th>Entrée</th>
          <th>Sortie</th>
          <th>Durée</th>
          <th>Statut</th>
          <th>Motif refus</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <AccesRow
          v-for="item in items"
          :key="item.id"
          :acces="item"
          @enregistrer-sortie="enregistrerSortie"
        />
        <tr v-if="items.length === 0">
          <td colspan="9" class="vide">Aucun accès trouvé.</td>
        </tr>
      </tbody>
    </table>

    <div class="pagination" v-if="totalPages > 1">
      <button :disabled="page <= 1" @click="rafraichir(page - 1)">Précédent</button>
      <span>Page {{ page }} / {{ totalPages }}</span>
      <button :disabled="page >= totalPages" @click="rafraichir(page + 1)">Suivant</button>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import AccesForm from "./AccesForm.vue";
import AccesRow from "./AccesRow.vue";

const items = ref([]);
const page = ref(1);
const pageSize = ref(20);
const totalPages = ref(1);
const chargement = ref(false);
const erreur = ref("");

const filtres = reactive({
  statut: "",
});

async function rafraichir(nouvellePage = page.value) {
  chargement.value = true;
  erreur.value = "";
  page.value = nouvellePage;

  try {
    const params = new URLSearchParams({
      page: String(page.value),
      pageSize: String(pageSize.value),
    });

    if (filtres.statut) params.append("statut", filtres.statut);

    const reponse = await fetch(`/api/acces?${params.toString()}`);

    if (!reponse.ok) throw new Error("Erreur lors du chargement de l'historique.");

    const data = await reponse.json();
    items.value = data.items;
    totalPages.value = data.totalPages;
  } catch (e) {
    erreur.value = e.message || "Erreur inconnue.";
  } finally {
    chargement.value = false;
  }
}

async function enregistrerSortie(accesId) {
  try {
    const reponse = await fetch("/api/acces/sortie", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ accesId }),
    });

    if (!reponse.ok) throw new Error("Impossible d'enregistrer la sortie.");

    await rafraichir();
  } catch (e) {
    erreur.value = e.message;
  }
}

onMounted(() => rafraichir());
</script>

<style scoped>
.acces-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  font-family: system-ui, sans-serif;
}
h2 {
  color: #106ad7;
}
.filtres select {
  padding: 0.4rem;
  border-radius: 4px;
  border: 1px solid #ccc;
}
table {
  width: 100%;
  border-collapse: collapse;
}
th {
  text-align: left;
  padding: 0.5rem;
  border-bottom: 2px solid #106ad7;
  font-size: 0.85rem;
  color: #333;
}
.vide {
  text-align: center;
  color: #888;
  padding: 1rem;
}
.pagination {
  display: flex;
  align-items: center;
  gap: 1rem;
}
.pagination button {
  padding: 0.3rem 0.7rem;
  border: 1px solid #106ad7;
  background: white;
  color: #106ad7;
  border-radius: 4px;
  cursor: pointer;
}
.pagination button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}
.erreur {
  color: #c0392b;
}
</style>