﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Comun;

namespace MaquinaDeEstados
{
    public class Maquina
    {
        public int iErr;
        public string sMsj;
        private int estadoInicial;
        //public const int eventoEnsamblaLote = 0;
        //public const int eventoEnviaAlSII = 2;
        //public const int eventoSIIAcepta = 3;
        //public const int eventoSIIRechazo = 5;
        //public const int eventoSIIReparo = 4;
        public const int eventoObtienePdf = 10;
        public const int eventoModificaFacturaEnLote = 9;
        public const int eventoNfseAnulada = 12;
        public const int eventoPrefecturaAcepta = 13;
        public const int eventoGeneraTxt = 14;
        //public const int eventoReceptorExcepcional = 15;
        //public const int eventoAcuseProducto = 11;
        //public const int eventoAcuseDocumento = 16;
        //public const int eventoEnviaMailACliente = 6;
        //public const int eventoEmiteLibro = 20;
        //public const int eventoCorrigeLibro =21;

        public const String estadoBaseEmisor = "emitido";
        public const String estadoBaseReceptor = "publicado";
        public const String binStatusBaseEmisor   = "000100000000";
        public const String binStatusBaseReceptor = "000100000000";

        public const String estadoBaseError = "error";
        public const String binStatusBaseAnulado = "000101001000";
        public const String idxBaseAnulado = "8";
        public const String estadoBaseAnulado = "nfse anulado";
        //private string[] _estados = { "anulado", "rechazado", "aceptado", "doc recibido", "publicado", "recibido", "rechazado SII", "con reparos SII", "aceptado SII", "enviado SII", "emitido", "no emitido", "prod recibido", "con error" };
        private string[] _estados = { "error", "rps ingresado", "rps modificado", "no emitido", "txt generado", "emitido", "nfse generado", "nfse impreso", "nfse anulado", "con cuotas", "remesa enviada", "bolb aceptado"};
        //                                0            1               2              3                4               5         6               7               8              9             10                 11           
        private Estado[] _Estados;

        private Transicion[] _matrizTransiciones;

        private string _binStatus;              //estado compuesto binario inicial
        private string _targetBinStatus;        //estado compuesto binario actual
        private int _idxSingleStatus;           //índice del status actual
        private Transicion _currentTransition;  //transición actual
        private short _voidStts;
        private int[] idxEBinario;
        private List<Transicion> paresOrdenados;
        private int maxLevel = 0;
        private int idxMaxLevel = 0;
        //**********************************************************
        #region Propiedades
        public string targetSingleStatus
        {
            get { return _estados[_currentTransition.destino]; }
        }
        public int idxTargetSingleStatus
        {
            get { return _idxSingleStatus; }
        }
        public string targetBinStatus
        {
            get { return _targetBinStatus; }
        }
        public string binStatus
        {
            get { return _binStatus; }
        }
        #endregion
        //**********************************************************

        public Maquina(string compoundedBinStatus, string idxSingleStatus, short voidStts, String tipoEstado, String claseMaquina)
        {
            try
            {
                _binStatus = compoundedBinStatus;
                _targetBinStatus = compoundedBinStatus;
                _idxSingleStatus = Convert.ToInt32(idxSingleStatus);

                if (tipoEstado.Equals("emisor"))
                    estadoInicial = binStatusBaseEmisor.IndexOf('1');
                else
                    estadoInicial = binStatusBaseReceptor.IndexOf('1');

                _currentTransition = new Transicion();
                _voidStts = voidStts;

                InicializaTransicionesDe(claseMaquina);
            
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Maquina(String tipoEstado, String claseMaquina)
        {
            try
            {
                if (tipoEstado.Equals("emisor"))
                {
                    _binStatus = binStatusBaseEmisor;
                    _targetBinStatus = binStatusBaseEmisor;
                }

                if (tipoEstado.Equals("receptor"))
                {
                    _binStatus = binStatusBaseReceptor;
                    _targetBinStatus = binStatusBaseReceptor;
                }

                _idxSingleStatus = _binStatus.IndexOf('1');
                estadoInicial = _idxSingleStatus;
                _currentTransition = new Transicion();
                _voidStts = 0;

                InicializaTransicionesDe(claseMaquina);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void InicializaTransicionesDe(String maquina)
        {

            if (maquina.Equals("DOCVENTA-contabilizado"))
            {
                _Estados = new Estado[]{ 
                new Estado("error", 0, -1),
                new Estado("rps ingresado", 1, -1),
                new Estado("rps modificado", 2, -1),
                new Estado("no emitido", 3, -1),
                new Estado("txt generado", 4, -1),
                new Estado("emitido", 5, -1),
                new Estado("nfse generado", 6, -1),
                new Estado("nfse impreso", 7, -1),
                new Estado("nfse anulado", 8, -1),
                new Estado("con cuotas", 9, -1),
                new Estado("remesa enviada", 10, -1),
                new Estado("bolb aceptado", 11, -1),
                };

                _matrizTransiciones = new Transicion[] {
                                    //Eventos de factura electrónica
                                    new Transicion(eventoGeneraTxt, "Genera archivo texto", "std", 3, 4),
                                    new Transicion(eventoGeneraTxt, "Genera archivo texto", "std", 4, 4),
                                    new Transicion(eventoPrefecturaAcepta, "Prefectura acepta", "std", 4, 5),
                                    new Transicion(eventoObtienePdf, "Descarga pdf", "std", 5, 6),
                                    new Transicion(eventoNfseAnulada, "Anula NFSe", "std", 5, 8),
                                    };
            };

            if (maquina.Equals("DOCVENTA-en lote"))
            {
                _Estados = new Estado[]{
                new Estado("error", 0, -1),
                new Estado("rps ingresado", 1, -1),
                new Estado("rps modificado", 2, -1),
                new Estado("no emitido", 3, -1),
                new Estado("txt generado", 4, -1),
                new Estado("emitido", 5, -1),
                new Estado("nfse generado", 6, -1),
                new Estado("nfse impreso", 7, -1),
                new Estado("nfse anulado", 8, -1),
                new Estado("con cuotas", 9, -1),
                new Estado("remesa enviada", 10, -1),
                new Estado("bolb aceptado", 11, -1),
                };

                _matrizTransiciones = new Transicion[] {
                                    new Transicion(eventoGeneraTxt, "Genera archivo texto", "std", 3, 4),
                                    new Transicion(eventoGeneraTxt, "Genera archivo texto", "std", 4, 4),
                                    new Transicion(eventoPrefecturaAcepta, "Prefectura acepta", "std", 4, 5),
                                    new Transicion(eventoModificaFacturaEnLote, "Modifica factura en lote", "std", 5, 3),
                                    new Transicion(eventoObtienePdf, "Descarga pdf", "std", 5, 6),
                                    new Transicion(eventoNfseAnulada, "Anula NFSe", "std", 5, 8),
                                    };
                
            };

        }

        /// <summary>
        /// Controla el ciclo de vida del objeto. 
        /// Determina la transición actual a partir del estado origen y del evento
        /// Si la condición de guarda retorna ok se puede ejecutar la acción
        /// 1/10/15 jcf Agrega try/catch
        /// </summary>
        /// <param name="evento"></param>
        /// <param name="docAnulado"></param>
        /// <param name="usuarioConAcceso"></param>
        /// <returns></returns>
        public bool Transiciona(int evento, int usuarioConAcceso)
        {
            iErr = 0;
            sMsj = string.Empty;
            bool guardaCondicion = false;

            try
            {
                var proximoPaso = _matrizTransiciones
                .Where(x => x.evento.Equals(evento))
                .Where(y => y.origen.Equals(_idxSingleStatus))
                .Select(y => y);

                if (proximoPaso.Count() > 0)
                {
                    //obtiene la transición actual
                    foreach (var tran in proximoPaso)
                        _currentTransition = tran;

                    //verifica la condición de guarda
                    guardaCondicion = _currentTransition.CondicionDeGuarda(_voidStts, usuarioConAcceso);
                    if (guardaCondicion)
                    {
                        char[] chBinStatus = _targetBinStatus.ToArray();
                        chBinStatus[_currentTransition.destino] = '1';
                        _targetBinStatus = new string(chBinStatus);

                        //Si la transición es a un subestado verificar que todos los subestados estén encendidos
                        if (_currentTransition.Tipo.Equals("sco"))
                        {
                            if (subEstadosEncendidos(_idxSingleStatus, _targetBinStatus, "sco"))
                                _idxSingleStatus = _currentTransition.destino;
                        }
                        else
                            _idxSingleStatus = _currentTransition.destino;
                    }
                    iErr = _currentTransition.iErr;
                    sMsj = _currentTransition.sMsj;
                }
                else
                {
                    iErr = -1;
                    sMsj = "Esta acción no corresponde (Evento: " + evento.ToString() + ")";
                }
                return guardaCondicion;

            }
            catch (Exception tr)
            {
                iErr++;
                sMsj = _matrizTransiciones == null ? "No existe transición para este tipo de documento. " : "Excepción desconocida en la transición del evento ";
                sMsj = sMsj + evento.ToString() + " [binStatus: " + _binStatus.ToString() + " targetBinStatus: " + _targetBinStatus.ToString() + " idxSingleStatus: " + _idxSingleStatus.ToString() + " estadoInicial: " + estadoInicial.ToString() + " voidStts: " + _voidStts.ToString() + "] " + tr.Message;
                return false;
            }
        }

        /// <summary>
        /// Retorna verdadero si todos los sub estados del tipo tipoT de binStatus están encendidos
        /// </summary>
        /// <param name="origen">Estado origen</param>
        /// <param name="binStatus"></param>
        /// <param name="tipoT">Tipo de transición con subestados</param>
        /// <returns></returns>
        private bool subEstadosEncendidos(int origen, string binStatus, String tipoT)
        {
            var componentesDelEstadoPadre = _matrizTransiciones
                .Where(y => y.origen.Equals(origen))
                .Where(x => x.Tipo.Equals(tipoT))
                .Select(y => y);

            bool todosEncendidos = true;
            foreach (var subEstado in componentesDelEstadoPadre)
                if (binStatus.Substring(subEstado.destino, 1).Equals("0"))
                {
                    todosEncendidos = false;
                    break;
                }

            return todosEncendidos;
        }

        /// <summary>
        /// Verifica si la transición del evento ya fue recorrida en eBinario
        /// </summary>
        /// <param name="evento"></param>
        /// <param name="eBinario"></param>
        /// <returns></returns>
        public bool transicionRecorrida(int evento, string eBinario)
        {
            bool existe = false;
            var transicionABuscar = _matrizTransiciones.Where (e => e.evento.Equals(evento));
            EstadoEnPalabras(eBinario); //carga pares ordenados
            foreach (Transicion t in transicionABuscar)
            {
                var par = paresOrdenados.Where(ori => ori.origen.Equals(t.origen))
                            .Where(des => des.destino.Equals(t.destino));
                foreach (var p in par)
                    existe = true;
            }
            return existe;
        }

        /// <summary>
        /// Traducción en palabras del estado binario 
        /// </summary>
        /// <param name="eBinario">Cadena de 12 bits de derecha a izquierda que indica los estados del documento.</param>
        /// <param name="compania">Parámetros de la compañía</param>
        /// <returns>Traducción del estado binario.</returns>
        public string EstadoEnPalabras(string eBinario)
        {
            try
            {
                Transicion parDondeInsertar = new Transicion();
                paresOrdenados = new List<Transicion>();
                idxEBinario = new int[eBinario.Length];
                //obtiene todos los pares origen - destinos del estado binario
                for (int i = 0; i < eBinario.Length; i++)
                {
                    if (eBinario[i] == '1')
                        idxEBinario[i] = i;
                    else
                        idxEBinario[i] = -1;
                }
                var paresDesordenados = _matrizTransiciones
                        .Where(x => idxEBinario.Contains(x.destino))
                        .Select(x => x);

                paresOrdenados.Add(new Transicion(0, "", "std", -1, estadoInicial));  //estado inicial
                idxMaxLevel = 0;
                maxLevel = 0;
                buscarSiguiente(estadoInicial, 0);

                //convierte los estados a palabras
                string enPalabras = string.Empty;
                String descripcionEstados = String.Empty;
                int anterior = paresOrdenados[idxMaxLevel].destino;

                for (int ind = idxMaxLevel; ind >= 0; ind-- )
                {
                    //No agregar palabras si es estado inicial y no tiene continuidad 
                    if (paresOrdenados[ind].destino != estadoInicial && paresOrdenados[ind].destino == anterior) 
                    {
                        descripcionEstados = paresOrdenados[ind].Tipo.Equals("sco") ? getDescripcionSubEstados(paresOrdenados[ind].origen, eBinario, "sco") : _estados[paresOrdenados[ind].destino].ToUpper(); 
                        enPalabras = descripcionEstados + ". " + enPalabras;
                    }
                    anterior = paresOrdenados[ind].origen;
                }

                return enPalabras;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Obtiene las cadenas de transiciones en la lista paresOrdenados
        /// </summary>
        /// <param name="destino">estado destino</param>
        /// <param name="nivel">nivel en la jerarquía de recursión</param>
        public void buscarSiguiente(int destino, int nivel)
        {
            var paresDesordenados = _matrizTransiciones.Where(x => idxEBinario.Contains(x.destino)).Select(x => x);
            foreach (var rec in paresDesordenados.Where(x => x.origen.Equals(destino)).Select(x => x))
            {
                bool existeLoop = paresOrdenados.Exists(x => x.destino.Equals(rec.destino));

                //No continuar si la transición de un estado es a si mismo o existe un loop entre estados
                if (!rec.origen.Equals(rec.destino) && !existeLoop)
                {
                    paresOrdenados.Add(new Transicion(nivel + 1, "", "std", rec.origen, rec.destino)); //utiliza el parámetro evento de la transición para guardar el nivel

                    if (nivel + 1 > maxLevel)
                    {
                        maxLevel = nivel + 1;
                        idxMaxLevel = paresOrdenados.Count() - 1;
                    }

                    //No continuar si la cadena ha llegado al estado señalado por _idxSingleStatus. 
                    //Esto previene la recursión infinita cuando hay estados que transicionan circularmente
                    if (!rec.destino.Equals(_idxSingleStatus))
                        buscarSiguiente(rec.destino, nivel + 1);
                }
            }
        }

        /// <summary>
        /// Obtiene las descripciones de los subestados encendidos en cualquier orden
        /// </summary>
        /// <param name="origen">Estado origen</param>
        /// <param name="binStatus"></param>
        /// <param name="tipoT">Tipo de transición con subestados</param>
        /// <returns></returns>
        private String getDescripcionSubEstados(int origen, string binStatus, String tipoT)
        {
            var componentesDelEstadoPadre = _matrizTransiciones
                .Where(y => y.origen.Equals(origen))
                .Where(x => x.Tipo.Equals(tipoT))
                .Select(y => y);

            String desc = String.Empty;
            foreach (var subEstado in componentesDelEstadoPadre)
                if (binStatus.Substring(subEstado.destino, 1).Equals("1"))
                    desc = _estados[subEstado.destino].ToUpper() + ". " + desc; 

            return desc;
        }


        public bool emitido(string eBinario)
        {
            return Utiles.Derecha(eBinario, 1).Equals("1");
        }
        public bool anulado(string eBinario)
        {
            return Utiles.Derecha(eBinario, 2)[0].Equals('1');
        }
        public bool impreso(string eBinario)
        {
            //Console.WriteLine(Utiles.Derecha(eBinario, 3)[0].Equals('1') + " - " + Utiles.Derecha(eBinario, 3)[0].Equals('1'));
            return Utiles.Derecha(eBinario, 3)[0].Equals('1');
        }
        public bool publicado(string eBinario)
        {
            return Utiles.Derecha(eBinario, 4)[0].Equals('1');
        }
        public bool enviado(string eBinario)
        {
            return Utiles.Derecha(eBinario, 5)[0].Equals('1');
        }
        public bool error(string eBinario)
        {
            return Utiles.Derecha(eBinario, 6)[0].Equals('1');
        }

    }
}