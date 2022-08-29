Les scripts les plus importants qui composent et font fonctionner Asteroid Effects sont les suivants:
GameManager, AsteroidManager, MoveAsteroid, ShipController et Bullet.

GameManager est le script qui va gérer le fonctionnement et l'état du jeu dans son intégralité. Il contrôle aussi
le bullet time et ses effets visuels et sonores, le découpage de la musique en temps en suivant le tempo, le 
début de partie et le GameOver et l'affichage du score.

AsteroidManager s'occupe de faire apparaître les gros astéroides et de leur appliquer les shaders d'apparition
ainsi que les valeurs de base.

Astéroides qui possèdent le script MoveAsteroid, qui s'occupe de gérer le mouvement de ces derniers, et en cas
d'explosion, gérer l'explosion et ses effets, l'apparition des astéroides plus petits et leurs valeurs (vitesse,
direction, score). MoveAsteroid contrôle aussi les effets d'animation des astéroides (bump, couleur) en fonction
des tempos recus par le GameManager.

ShipController comme son nom l'indique, contrôle tout ce qui est lié au vaisseau, mouvement, vie et position sur 
l'ecran. Le script gère aussi tir et bullet time.

Et lors du tir, sont instanciés les bullet avec le Script bullet qui va simplement gérer sa trajectoire et
envoyer une info à l'astéroide sur lequel il se collera lors de la collision, pour activer l'explosion au tempo
prochain.