LOGROS Y MISIONES — ASSETS PIXEL (Fase 9, identidad pixel)
==========================================================

Íconos pixel-art con FONDO TRANSPARENTE. Son 6 assets por CATEGORÍA (no uno por
logro): varios logros comparten el mismo ícono según su tema. Recortados del sheet
"assets-fitcoach" que aportó el usuario.

Assets (6):
  entrenos.png    -> mancuerna   (entrenos y misión "Constancia semanal")
  peso.png        -> disco       (peso y misión "Control de peso")
  rachas.png      -> rayo        (rachas y logro "Semana de fuego")
  records.png     -> flecha      (récords personales)
  objetivo.png    -> diana       (objetivo definido)
  diario.png      -> checklist   (diario y misión "Come con cabeza")

El mapeo logro/misión -> asset vive en Views/Gamificacion/Index.cshtml
(funciones IconoLogro / IconoMision). Los bloqueados/no cumplidos se muestran en
gris (filtro CSS .logro-medal.is-locked); los desbloqueados/cumplidos brillan y
laten (.is-unlocked). Ya no se usan emojis ni el canvas de logros.js.
