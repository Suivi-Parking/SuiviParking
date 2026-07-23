<template>
  <tr class="acces-row" :class="statutClasse">
    <td>{{ acces.id }}</td>
    <td>{{ acces.vehiculeImmatriculation }}</td>
    <td>{{ acces.employeNom }}</td>
    <td>{{ formaterDate(acces.dateHeureEntree) }}</td>
    <td>{{ acces.dateHeureSortie ? formaterDate(acces.dateHeureSortie) : "—" }}</td>
    <td>{{ acces.dureeFormatee || "—" }}</td>
    <td>
      <span class="badge">{{ libelleStatut }}</span>
    </td>
    <td>{{ acces.motifRefus || "—" }}</td>
    <td>
      <button
        v-if="acces.statut === 'EN_COURS'"
        class="btn-sortie"
        @click="$emit('enregistrer-sortie', acces.id)"
      >
        Enregistrer sortie
      </button>
    </td>
  </tr>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  acces: {
    type: Object,
    required: true,
  },
});

defineEmits(["enregistrer-sortie"]);

const libelles = {
  EN_COURS: "En cours",
  SORTI: "Sorti",
  REFUSE: "Refusé",
};

const libelleStatut = computed(() => libelles[props.acces.statut] || props.acces.statut);

const statutClasse = computed(() => `statut-${props.acces.statut.toLowerCase()}`);

function formaterDate(valeur) {
  if (!valeur) return "—";
  return new Date(valeur).toLocaleString("fr-FR");
}
</script>

<style scoped>
.acces-row td {
  padding: 0.5rem;
  border-bottom: 1px solid #eee;
  font-size: 0.9rem;
}
.badge {
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 600;
}
.statut-en_cours .badge {
  background: #d4edda;
  color: #1e7e34;
}
.statut-sorti .badge {
  background: #e2e3e5;
  color: #383d41;
}
.statut-refuse .badge {
  background: #f8d7da;
  color: #c0392b;
}
.btn-sortie {
  padding: 0.3rem 0.6rem;
  background: #106ad7;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.8rem;
}
</style>
