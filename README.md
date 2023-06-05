
# E2.0.1 Corrección-Examen
Envía o repositorio coa rama de corrección co exame ben feito.

## E2.0 Exame: equipos en rede

(1 punto) Uso axeitado de variables, métodos, orde, eficiencia e calidade do código. Realización dun pequeno informe con capturas dos pasos e brevísimas explicacións para cada apartado.
(2 puntos) Crea desde cero un xogo 3D semellante ao getting started do manual de netcode. Isto é, un xogo no que hai un plano e sobre él os playeres, que serán unha cápsula que se move nas catro direccións sobre o plano utilizando teclas (frechas, ASDW ou ambas). Debes usar Network transform.
(1 punto) Fai que cando se spawnea apareza nun punto aleatorio da parte central do plano (a que está etiquetada como "sen equipo" no esquema de abaixo) e de cor branca. Crea un botón chamado "mover a inicio" que leve ao player á parte central do plano, fai que a tecla "m" teña a mesma acción. Se estamos executando en servidor esa acción debe ser feita por todos os players.
(1 punto) Os players sen equipo (os que están na parte central do plano) teñen cor branca. Se o player se move para a zona do Equipo 1 (ver esquema abaixo) poñerase de cor vermello e os players que se poñan no Equipo 2 collerán cor azul. Se volven á parte central volverán a estar sen equipo e por tanto volverán ser de cor branca.
(2 puntos) Fai que en cada equipo só poida haber como máximo dous players (os sen equipo non teñen limitación). E en caso de que un equipo esté cheo, só se poderán mover os players dese equipo e os outros so poderán moverse de novo cando o equipo non esté cheo (utiliza ClientRPC para avisalos). Prográmao de tal xeito que poidas mudar facilmente o número máximo de players nun equipo no futuro. 
(2 puntos) Fai que o equipo 1 teña as cores vermella, laranxa e rosa, e o equipo 2 teñas as cores azul escuro, violeta e azul claro. Cando un xogador entre nun equipo collerá unha das cores aleatorias que non estén collidas. 
(1 punto) Utiliza OnValueChange en algún lugar no que sexa necesario.

Entregables:

Documento do informe

Código do Player e do Manager

Link ao repositorio co código completo

![image](https://github.com/9RACHA/E2.0.1-Correccion-Examen/assets/66274956/373cfda9-e652-4a28-aa48-b0c414af421f)

## Informe
Instalar el paquete netcode for gameobjects poniendo en el package manager: com.unity.netcode.gameobjects (Agregar paquete por nombre)
![image](https://github.com/9RACHA/E2.0.1-Correccion-Examen/assets/66274956/43bc7f74-38f9-4e16-b541-3319c23b3a48)

En el script GameManager simplemente se añadio el boton Mover, que hace que se llame al metodo Mover() del script Player y dentro de este si el cliente es propietario hara que se llame a EnviarPosicionServerRpc() este al ser un metodo RPC enviará la posicion actual del jugador al servidor dentro de este se definira la Position como 
ObtenerPosicionCentralEnPlano() y este es un metodo estatico que pondrá al player siempre en el centro de la escena.

Al comenzar el juego desde Unity como Host 
![Host](https://github.com/9RACHA/Examen--equipos-en-rede/assets/66274956/d93e846d-19f8-4743-8a3b-f79c70186b6b)
El player comenzara en el centro Sin un equipo asignado y con el color Blanco. Se llama al metodo OnNetworkSpawn()

En el Update se comprobara constantemente el numero de jugadores en el equipo Azul y Rojo en este caso no hay ningun player.
![InicioConsola](https://github.com/9RACHA/Examen--equipos-en-rede/assets/66274956/9029b1b3-f8a8-41fb-9bf9-da5474337800)
Tambien mediante OnCollisionEnter se comprobara, tanto si es propietario como cliente si el player esta tocando el tag asignado, en este caso "SinEquipo" necesitará un collider asignado en la jerarquia de Unity y un Rigidbody.

Al desplazarse hacia el tag Azul 
![image](https://github.com/9RACHA/Examen--equipos-en-rede/assets/66274956/d433bf0e-93dd-421c-bc0b-09514a19ca5e)
![image](https://github.com/9RACHA/Examen--equipos-en-rede/assets/66274956/a20029ff-ec99-48e6-ae4b-c684b834dce3)
![image](https://github.com/9RACHA/Examen--equipos-en-rede/assets/66274956/13265db2-9bdb-4d34-8a05-b4eb914e055b)
Mediante el metodo ColorPlayerValuechanged se podra obtener el color en RGBA desde Blanco hacia Azul en este caso.

Al hacer la build y mover los 2 player hacia la derecha
![image](https://github.com/9RACHA/Examen--equipos-en-rede/assets/66274956/981617f4-aec9-4c56-9c0b-ead3507dc35c)
Los jugadores Pasaran en este caso de Blanco SinEquipo a Rojo -> Equipo Rojo y Blanco Sin Equipo a Rosa -> Equipo Rojo respectivamente. Por tanto habra 2 jugadores en dicho equipo.
![image](https://github.com/9RACHA/Examen--equipos-en-rede/assets/66274956/886a2c60-956d-4d7e-a3ad-0aea6d004238)





 
