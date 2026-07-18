<template>
  <form class="acces-form" @submit.prevent="soumettre">
    <h3>{{ mode === 'entree' ? "Autoriser une entrée" : "Enregistrer une sortie" }}</h3>

    <div class="onglets">
      <button type="button" :class="{ actif: mode === 'entree' }" @click="mode = 'entree'">
        Entrée
      </button>
      <button type="button" :class="{ actif: mode === 'sortie' }" @click="mode = 'sortie'">
        Sortie
      </button>
    </div>

    <template v-if="mode === 'entree'">
      <label>
        Véhicule (ID)
        <input v-model.number="formEntree.vehiculeId" type="number" required min="1" />
      </label>

      <label>
        Parking (ID)
        <input v-model.number="formEntree.parkingId" type="number" required min="1" />
      </label>

      <label>
        Abonnement (ID)
        <input v-model.number="formEntree.abonnementId" type="number" min="1" />
      </label>
    </template>

    <template v-else>
      <label>
        Accès (ID)
        <input v-model.number="formSortie.accesId" type="number" required min="1" />
      </label>
    </template>

    <p v-if="erreur" class="erreur">{{ erreur }}</p>
    <p v-if="succesMessage" class="succes">{{ succesMessage }}</p>

    <button type="submit" :disabled="envoiEnCours">
      {{ envoiEnCours ? "Envoi..." : "Valider" }}
    </button>
  </form>
</template>

<script setup>
import { ref, reactive } from "vue";

const emit = defineEmits(["acces-cree", "acces-modifie"]);

const mode = ref("entree");
const envoiEnCours = ref(false);
const erreur = ref("");
const succesMessage = ref("");

const formEntree = reactive({
  vehiculeId: null,
  parkingId: null,
  abonnementId: null,
});

const formSortie = reactive({
  accesId: null,
});

async function soumettre() {
  erreur.value = "";
  succesMessage.value = "";
  envoiEnCours.value = true;

  try {
    if (mode.value === "entree") {
      const reponse = await fetch("/api/acces/entree", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formEntree),
      });

      const data = await reponse.json();

      if (!reponse.ok) {
        erreur.value = data.message || "Accès refusé.";
        return;
      }

      succesMessage.value = "Entrée autorisée avec succès.";
      emit("acces-cree", data);
      formEntree.vehiculeId = null;
      formEntree.parkingId = null;
      formEntree.abonnementId = null;
    } else {
      const reponse = await fetch("/api/acces/sortie", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formSortie),
      });

      const data = await reponse.json();

      if (!reponse.ok) {
        erreur.value = data.message || data || "Erreur lors de l'enregistrement de la sortie.";
        return;
      }

      succesMessage.value = "Sortie enregistrée avec succès.";
      emit("acces-modifie", data);
      formSortie.accesId = null;
    }
  } catch (e) {
    erreur.value = "Erreur réseau : impossible de contacter le serveur.";
  } finally {
    envoiEnCours.value = false;
  }
}
</script>

<style scoped>
.acces-form {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  max-width: 360px;
  padding: 1rem;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
}
.onglets {
  display: flex;
  gap: 0.5rem;
}
.onglets button {
  flex: 1;
  padding: 0.4rem;
  border: 1px solid #106ad7;
  background: white;
  color: #106ad7;
  border-radius: 6px;
  cursor: pointer;
}
.onglets button.actif {
  background: #106ad7;
  color: white;
}
label {
  display: flex;
  flex-direction: column;
  font-size: 0.85rem;
  gap: 0.25rem;
}
input {
  padding: 0.4rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}
button[type="submit"] {
  padding: 0.5rem;
  background: #106ad7;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}
button[type="submit"]:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.erreur {
  color: #c0392b;
  font-size: 0.85rem;
}
.succes {
  color: #1e8449;
  font-size: 0.85rem;
}
</style>