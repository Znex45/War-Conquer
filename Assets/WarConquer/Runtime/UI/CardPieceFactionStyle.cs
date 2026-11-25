using UnityEngine;

namespace WarConquer
{
    public static partial class CardPieceModels
    {
        sealed partial class Sculptor
        {
            // Applied after each card's anatomy and equipment have been seated. The style
            // remains entirely in the presentation hierarchy, before idle poses are captured.
            void ApplyFactionStyle()
            {
                switch(card.leader)
                {
                    case "SAHRIA":ApplySahriaStyle();break;
                    case "FAUNAR":ApplyFaunarStyle();break;
                    case "ZUKGROK":ApplyZukgrokStyle();break;
                }
            }

            void RefineTheme()
            {
                switch (id)
                {
                    case "recolector-de-esporas":
                        ShapeAndColor(0xB88249, V(1.02f,1.08f,1.03f),"Cesta lateral del recolector");
                        ShapeAndColor(0xD794DE,V(1.09f,1.04f,1.09f),"Frasco de esporas");
                        break;
                    case "hongo-explorador":
                        ShapeAndColor(0x3D7774,V(1.08f,1.02f,1),"Capa de hoja del explorador");
                        ShapeAndColor(0xFFD377,V(1.09f,1.06f,1.09f),"Farol de luciérnaga");
                        break;
                    case "larva-micelial":
                        ShapeAndColor(0x9BB976,V(.97f,1.06f,1.05f),"Segmento de larva");
                        ShapeAndColor(0xDDD468,V(1.13f,1.13f,1.13f),"Espora sensorial");
                        break;
                    case "bestia-micelial":
                        ShapeAndColor(0x527A56,V(1.06f,1.09f,1.07f),"Coraza micelial bestial");
                        ShapeAndColor(0x8B4C70,V(1.09f,1.09f,1.09f),"Sombrero esculpido");
                        break;
                    case "guardian-del-bosque":
                        ShapeAndColor(0x936645,V(1.05f,1.07f,1),"Escudo de corteza");
                        ShapeAndColor(0x9BC775,V(1.07f,1.07f,1),"Emblema del bosque");
                        break;
                    case "cazador-de-esporas":
                        ShapeAndColor(0x476E49,V(1.1f,1.04f,1),"Camuflaje de cazador");
                        ShapeAndColor(0xD6DD76,V(1.1f,1.1f,1.13f),"Punta de espora");
                        break;
                    case "chaman-micelial":
                        ShapeAndColor(0x85539E,V(1.05f,1.09f,1.05f),"Capucha de laminillas");
                        ShapeAndColor(0xD2A6EF,V(1.11f,1.09f,1.04f),"Talismán de sueño");
                        break;
                    case "coloso-de-esporas":
                        ShapeAndColor(0x5C726F,V(1.04f,1.05f,1.08f),"Hombrera de coloso");
                        ShapeAndColor(0xA3EDB1,V(1.15f,1.06f,1.13f),"Fisura luminosa del coloso");
                        break;
                    case "matriarca-micelial":
                        ShapeAndColor(0xA85F81,V(1.06f,1.04f,1.06f),"Capucha de laminillas");
                        ShapeAndColor(0xDBA9BE,V(1.06f,1.07f,1.08f),"Vaina de cría");
                        break;
                    case "ancestro-micelial":
                        ShapeAndColor(0x698961,V(1.12f,1.1f,1.04f),"Copa ancestral");
                        ShapeAndColor(0xC1B396,V(1.04f,1.09f,1),"Barba de raíces");
                        break;
                    case "ciervo-de-esporas":
                        ShapeAndColor(0xD5BB86,V(1.04f,1.06f,1.04f),"Asta bifurcada","Asta exterior");
                        ShapeAndColor(0xC9E889,V(1.13f,1.13f,1.13f),"Espora del asta");
                        break;
                    case "espora":
                        ShapeAndColor(0xA077B7,V(1.11f,1.05f,1.1f),"Capucha de laminillas");
                        ShapeAndColor(0xF1D0E4,V(1.10f,1.20f,1.10f),"Lóbulo de espora","Motas grandes del sombrero");
                        break;
                    case "nomada-de-arena":
                        ShapeAndColor(0x9A5A43,V(1.04f,1.10f,1.07f),"Turbante del nómada","Velo contra la arena");
                        ShapeAndColor(0xD0A072,V(1.10f,1.05f,1.05f),"Bolsa de travesía");
                        break;
                    case "escorpion-solar":
                        ShapeAndColor(0xBF713B,V(1.07f,1.05f,1.1f),"Pinza","Dedo fijo de pinza","Dedo móvil de pinza");
                        ShapeAndColor(0xFFD364,V(1.14f,1.14f,1.14f),"Gota de veneno solar");
                        break;
                    case "guardian-del-obelisco":
                        ShapeAndColor(0xAA864F,V(1.05f,1.08f,1.06f),"Escudo torre del obelisco","Borde del escudo torre");
                        ShapeAndColor(0x47949A,V(1.06f,1.08f,1),"Campo del escudo torre");
                        break;
                    case "sacerdote-solar":
                        ShapeAndColor(0xEEE0B4,V(1.05f,1.03f,1.04f),"Túnica modelada","Pergamino sagrado");
                        ShapeAndColor(0xDF9D54,V(1.13f,1.1f,1.07f),"Piedra del rito solar");
                        break;
                    case "bestia-de-las-dunas":
                        ShapeAndColor(0xBA8D5D,V(1.08f,1.06f,1.1f),"Placa de hombro de arena","Placa de roca");
                        ShapeAndColor(0xE2C596,V(1.09f,1.12f,1.06f),"Cuerno de bestia de dunas");
                        break;
                    case "avatar-del-sol":
                        ShapeAndColor(0xF3B957,V(1.12f,1.08f,1),"Rayo del avatar solar");
                        ShapeAndColor(0xD87D3F,V(1.13f,1.06f,1),"Cola de luz del avatar");
                        break;
                    case "guardian-del-oasis":
                        ShapeAndColor(0x477E73,V(1.09f,1.08f,1.02f),"Caparazón del oasis");
                        ShapeAndColor(0x92CDC0,V(1.11f,1.13f,1.09f),"Placa turquesa del oasis");
                        break;
                    case "halconero-solar":
                        ShapeAndColor(0xA56D43,V(1.15f,1.16f,1.12f),"Torso del halcón");
                        ShapeAndColor(0xE8CA8B,V(1.12f,1.08f,1),"Pluma del halcón compañero");
                        break;
                    case "escarabajo-de-cobre":
                        ShapeAndColor(0xB36A4B,V(1.1f,1.08f,1.06f),"Élitro de cobre");
                        ShapeAndColor(0xD99569,V(1.10f,1.12f,1.07f),"Cuerno de escarabajo");
                        break;
                    case "emperador-de-las-dunas":
                        ShapeAndColor(0xA85443,V(1.1f,1.06f,1),"Manto imperial amplio","Estandarte del emperador");
                        ShapeAndColor(0xE8B75F,V(1.1f,1.12f,1),"Sol heráldico","Cresta imperial");
                        break;
                    case "rabbit-token":
                        ShapeAndColor(0xDECCA0,V(1.10f,1.1f,1.10f),"Cabeza animal","Torso animal");
                        ShapeAndColor(0xC9918C,V(1.1f,1.12f,1.04f),"Interior de oreja");
                        break;
                    case "battle-ant-token":
                        ShapeAndColor(0x694230,V(1.02f,1.06f,1.06f),"Abdomen segmentado","Tórax","Cabeza de insecto");
                        ShapeAndColor(0xB9824C,V(1.11f,1.09f,1.09f),"Mandíbula de hormiga");
                        break;
                    case "frog-token":
                        ShapeAndColor(0x7EAB50,V(1.1f,1.06f,1.07f),"Cuerpo anfibio","Cabeza ancha");
                        ShapeAndColor(0xC4D273,V(1.08f,1.06f,1.13f),"Pecho crema","Ojo elevado");
                        break;
                    case "mushroom-token":
                        ShapeAndColor(0xB98762,V(1.12f,1.11f,1.14f),"Capucha de laminillas");
                        ShapeAndColor(0xEAC49F,V(1.09f,1.06f,1.07f),"Motas grandes del sombrero");
                        break;
                    case "firefly-token":
                        ShapeAndColor(0xFFCE64,V(1.15f,1.12f,1.17f),"Órgano luminiscente");
                        ShapeAndColor(0xCBC19E,V(1.08f,1.09f,1),"Ala de nácar");
                        break;
                    case "alligator-token":
                        ShapeAndColor(0x729947,V(1.1f,.98f,1.06f),"Lomo escamado","Hocico");
                        ShapeAndColor(0xA5B85C,V(1.12f,1.09f,1.07f),"Motas de caimán joven","Cresta dorsal");
                        break;

                    case "semillero-micelial":
                        ShapeAndColor(0xA27B62,V(1.08f,1.08f,1.05f),"Maceta de espora");
                        ShapeAndColor(0xAC7B8F,V(1.09f,1.12f,1.09f),"Sombrero esculpido");
                        break;
                    case "nido-de-esporas":
                        ShapeAndColor(0x8D654F,V(1.10f,1.07f,1.09f),"Cestería del nido");
                        ShapeAndColor(0xC2A5CA,V(1.1f,1.12f,1.1f),"Saco de espora","Huevo central");
                        break;
                    case "santuario-del-bosque":
                        ShapeAndColor(0x84A168,V(1.10f,1.10f,1.05f),"Copa protectora");
                        ShapeAndColor(0xDCE99C,V(1.10f,1.12f,1.10f),"Semilla sagrada");
                        break;
                    case "jardin-micelial":
                        ShapeAndColor(0xBC79A6,V(1.11f,1.14f,1.11f),"Pétalo de flor");
                        ShapeAndColor(0x9C728D,V(1.09f,1.11f,1.09f),"Sombrero esculpido");
                        break;
                    case "red-micelial":
                        ShapeAndColor(0x8BCCAA,V(1.1f,1.08f,1.1f),"Conector luminoso","Enlace de red");
                        ShapeAndColor(0xC5F4CE,V(1.09f,1.12f,1.09f),"Nodo de micelio","Nexo de la red");
                        break;
                    case "torre-de-esporas":
                        ShapeAndColor(0x679183,V(1.08f,1.08f,1.07f),"Corona esporífera");
                        ShapeAndColor(0xC3DF96,V(1.13f,1.15f,1.13f),"Espora preparada");
                        break;
                    case "corazon-del-hongo":
                        ShapeAndColor(0xBA6483,V(1.08f,1.10f,1.08f),"Ventrículo izquierdo","Ventrículo derecho","Punta del corazón");
                        ShapeAndColor(0xE6A497,V(1.15f,1.08f,1.10f),"Vena luminosa");
                        break;
                    case "santuario-del-sueno":
                        ShapeAndColor(0xB1B5E5,V(1.1f,1.04f,1.04f),"Luna creciente");
                        ShapeAndColor(0xDABBEE,V(1.14f,1.12f,1.11f),"Estrella del sueño");
                        break;
                    case "arbol-madre":
                        ShapeAndColor(0x7D9B62,V(1.08f,1.11f,1.08f),"Copa maternal");
                        ShapeAndColor(0xE0CC78,V(1.13f,1.09f,1.1f),"Semilla madre");
                        break;
                    case "obelisco-solar":
                        ShapeAndColor(0xB98045,V(1.07f,1.08f,1.06f),"Disco solar de bronce");
                        ShapeAndColor(0xF4BF65,V(1.08f,1.11f,1.05f),"Rayo del obelisco","Ojo del sol");
                        break;
                    case "campamento-nomada":
                        ShapeAndColor(0xA8654D,V(1.1f,1.11f,1.05f),"Banderín de ruta");
                        ShapeAndColor(0x997857,V(1.1f,1.08f,1.1f),"Rueda de caravana");
                        break;
                    case "santuario-de-arena":
                        ShapeAndColor(0xC99757,V(1.08f,1.1f,1.08f),"Cúpula de arena");
                        ShapeAndColor(0xC6B187,V(1.11f,1.04f,1.1f),"Columna del santuario");
                        break;
                    case "obelisco-de-ruptura":
                        ShapeAndColor(0x8D786C,V(1.08f,1.07f,1.09f),"Mitad del obelisco roto");
                        ShapeAndColor(0xF38B64,V(1.12f,1.07f,1.12f),"Fisura de ruptura","Núcleo de fractura");
                        break;
                    case "templo-de-ceniza":
                        ShapeAndColor(0x58515B,V(1.08f,1.04f,1.10f),"Cornisa de escoria","Pirámide de escoria");
                        ShapeAndColor(0xFF9453,V(1.13f,1.13f,1.13f),"Llama del templo");
                        break;
                    case "trono-solar":
                        ShapeAndColor(0xE1AE54,V(1.07f,1.09f,1.06f),"Cabeza de león del trono","Trono de oro");
                        ShapeAndColor(0xAE6548,V(1.08f,1.07f,1),"Alfombra solar");
                        break;
                    case "arena-profunda":
                        ShapeAndColor(0xA47145,V(1.07f,1.10f,1.06f),"Labio de arena hundida");
                        ShapeAndColor(0x735441,V(1.1f,1.07f,1.11f),"Fosa de arena oscura");
                        break;
                    case "dunas-movedizas":
                        ShapeAndColor(0xD6A977,V(1.04f,1.13f,1.04f),"Cresta de duna");
                        ShapeAndColor(0xF2D396,V(1.12f,1.09f,1.06f),"Viento sobre arena","Línea de cresta");
                        break;
                    case "oasis-de-cristal":
                        ShapeAndColor(0x71D6D4,V(1.10f,1.14f,1.08f),"Cristal del oasis");
                        ShapeAndColor(0x58ADB1,V(1.1f,1.03f,1.10f),"Agua de oasis");
                        break;
                    case "obelisco-de-arena":
                        ShapeAndColor(0xC2A077,V(1.07f,1.07f,1.08f),"Obelisco emergente");
                        ShapeAndColor(0xEFCC8F,V(1.07f,1.10f,1.07f),"Punta de obelisco de arena","Glifo de arena");
                        break;
                }
            }

            void ShapeAndColor(int rgb, Vector3 stretch, params string[] names)
            {
                foreach (var renderer in model.GetComponentsInChildren<MeshRenderer>())
                {
                    if (System.Array.IndexOf(names, renderer.name) < 0) continue;
                    var part = renderer.transform;
                    part.localScale = Vector3.Scale(part.localScale, stretch);
                    m.Paint(renderer, Pigment(rgb));
                }
            }
        }
    }
}
