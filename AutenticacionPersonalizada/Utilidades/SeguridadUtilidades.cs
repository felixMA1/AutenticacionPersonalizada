﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AutenticacionPersonalizada.Utilidades
{
    public class SeguridadUtilidades
    {

        public static String GetSha1(String texto)
        {
            //crear SHA
            var sha = SHA1.Create();
            //crear codificador para el formato de los caracteres
            UTF8Encoding encoding = new UTF8Encoding();
            //array de bytes porque la clase SHA devuelve un array de bytes
            byte[] datos;
            //para formatear la cadena
            StringBuilder builder = new StringBuilder();
            //generar HASH para un conjunto de bytes
            datos = sha.ComputeHash(encoding.GetBytes(texto));

            for (int i = 0; i < datos.Length; i++)
            {
                //{0:x2} -> genera un hexadecimal de 2 digitos por byte
                builder.AppendFormat("{0:x2}", datos[i]);
            }

            return builder.ToString();
        }


        /*
            Cifrado asimetrico: Cuando tenga q compartir el contenido con otros.
            Cifrado simetrico:    
        */

        public static byte[] GeyKey(String txt)
        {
            //Genera una password (array de bytes) de forma aleatoria
            //desde un origen. El origen es txt. Null es el salt;un numero para el calculo aleatorio.
            return new PasswordDeriveBytes(txt,null).GetBytes(32);
        }

        public static String Cifrar(String contenido, String clave)
        {
            //tipo de codificacion
            var encoding = new UTF8Encoding();
            //determina cual es el algoritmo de cifrado
            var cripto = new RijndaelManaged();
            //texto q yo quiero cifrar
            byte[] cifrado;
            //array que contiene el IV mas el txt cifrado
            byte[] retorno;
            //convierte la clave en array de bytes
            byte[] key = GeyKey(clave);

            cripto.Key = key;
            //genera array de inicializacion
            cripto.GenerateIV();
            //el texto a encriptar lo convierte en array de byte
            byte[] aEncriptar = encoding.GetBytes(contenido);
            //con el IV y la clave crea un encriptador al final del bloque( el q transforma, dnd empieza, dnd termina)
            cifrado = cripto.CreateEncryptor().TransformFinalBlock(aEncriptar, 0, aEncriptar.Length);
            //prepara lo q va a devolver. Array de bytes q esta compuesto de la longitud del IV mas la longitud de lo q he cifrado
            retorno = new byte[cripto.IV.Length + cifrado.Length];
            //me copias el array del IV empezando en la longitud 0
            cripto.IV.CopyTo(retorno, 0);
            cifrado.CopyTo(retorno, cripto.IV.Length);

            return Convert.ToBase64String(retorno);
        }

        public static String DesCifrar(byte[] contenido, String clave)
        {
            //tipo de codificacion
            UTF8Encoding encoding = new UTF8Encoding();
            var cripto = new RijndaelManaged();
            //IV vacio del tamaño del IV por defecto del Rijndael
            var ivTemp = new byte[cripto.IV.Length];
            //la clave tiene q ser la misma para cifrar y descifrar 
            var key = GeyKey(clave);
            //creo un array cifrado. La longitud es tdo lo q tengo menos el IV 
            var cifrado = new byte[contenido.Length - ivTemp.Length];

            cripto.Key = key;
            //(de dnd voy a sacar los datos, dnd voy a escribir los datos, cuantos datos voy a escribir)
            Array.Copy(contenido,ivTemp,ivTemp.Length);
            //(de dnd voy a sacar los datos, dnd quieres empezar a copiar, en dnd, desde dnd, hasta dnd)
            Array.Copy(contenido,ivTemp.Length,cifrado,0,cifrado.Length);

            cripto.IV = ivTemp;         
            var descifrado = cripto.CreateDecryptor().TransformFinalBlock(cifrado, 0, cifrado.Length);

            return encoding.GetString(descifrado);
        }
    }
}
