parkLine frontend (Vue 3 + Vite + Tailwind)

Instructions:
1. Ouvrez un terminal dans Documents/parkini/frontend
2. Exécutez `npm install` pour installer les dépendances.
3. Lancez `npm run dev` pour démarrer le serveur de développement Vite.

Remarques:
- Le frontend appelle `/api/entreprises`. Assurez-vous que votre backend .NET est démarré et accessible au même origin (ou configurez un proxy dans vite.config.js).
- Les styles utilisent Tailwind; si vous changez la palette, modifiez tailwind.config.cjs ou les variables CSS dans src/assets/styles.css.
