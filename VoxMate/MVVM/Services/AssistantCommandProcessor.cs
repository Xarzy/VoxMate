using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using VoxMate.MVVM.Views;

namespace VoxMate.MVVM.Services
{
    public interface IAssistantCommandProcessor
    {
        string Process(string text);
    }

    public class AssistantCommandProcessor : IAssistantCommandProcessor
    {
        private static readonly string HelpText = @"Puedo ayudarte con las siguientes tareas:

- Conversación básica: saludos variables, despedidas y presentación.
- Memoria simple: puedo recordar tu nombre si me dices ""Me llamo ..."".
- Hora y fecha actuales.
- Números aleatorios: pedir un número al azar o dentro de un rango.
- Bromas y chistes generales.
- Cálculos matemáticos:
  • Suma, resta, multiplicación y división.
  • Expresiones aritméticas (ej.: 2+3*4).
  • Porcentajes (ej.: ""¿cuánto es el 20% de 50?"").
  • Raíz cuadrada y potencias.
- Conversión de unidades:
  • Kilómetros ↔ millas.
  • Metros ↔ kilómetros.
- Conversión de temperatura:
  • Celsius ↔ Fahrenheit.

Cómo usar:
- Di el comando y pulsa ""Detener"" para procesarlo, o ""Cancelar"" para descartar.

Ejemplos:
- ""Hola""
- ""Me llamo Carlos""
- ""Dame un número aleatorio entre 1 y 100""
- ""Cuéntame un chiste""
- ""Convierte 3 km a metros""
- ""¿Cuánto es 25% de 80?""
- ""Raíz de 16""
- ""¿Qué puedes hacer?""";


        private static readonly string[] Jokes =
        {
            "¿Sabes por qué los esqueletos no pelean entre ellos? Porque no tienen agallas.",
            "Intenté ser organizado… pero se me desordenaron las ideas.",
            "Mi reloj me dijo que necesitaba tiempo.",
            "Hoy iba a contar un chiste sobre pizza, pero es demasiado cheesy.",
            "¿Por qué el libro de matemáticas estaba triste? Porque tenía muchos problemas.",
            "Quise hacer ejercicio, pero el sofá me habló con más convicción.",
            "Dicen que el dinero no da la felicidad… pero ayuda con el WiFi."
        };



        public string AssistantName { get; private set; } = "VoxMate";
        private string? UserName { get; set; }

        private static readonly Random Random = new();

        private static string Pick(string[] options)
        {
            return options[Random.Next(options.Length)];
        }

        private static string GetGreeting()
        {
            var hour = DateTime.Now.Hour;

            string[] morning =
            {
                "Buenos días",
                "Hola, buenos días",
                "¡Buen día!",
                "Hola, que tengas un gran día"
            };

            string[] afternoon =
            {
                "Buenas tardes",
                "Hola, buenas tardes",
                "¿Qué tal la tarde?",
                "Hola, espero que estés teniendo una buena tarde"
            };

            string[] night =
            {
                "Buenas noches",
                "Hola, buenas noches",
                "¿Cómo va la noche?",
                "Hola, espero que estés teniendo una buena noche"
            };

            if (hour >= 6 && hour < 12) return Pick(morning);
            if (hour >= 12 && hour < 20) return Pick(afternoon);
            return Pick(night);
        }

        private static readonly string[] Goodbyes =
        {
            "Hasta luego",
            "Nos vemos",
            "Que tengas un buen día",
            "Cuídate",
            "Hasta pronto",
            "¡Nos hablamos!"
        };


        public string Process(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "No se ha detectado texto para procesar.";

            var original = text.Trim();
            var lowered = original.ToLowerInvariant();

            // Intento de ayuda
            if (Regex.IsMatch(lowered, @"\b(ayuda|ayudar|ayudame|ayúdame|qué puedes hacer|que puedes hacer|qué puede hacer|que puede hacer|comandos|tareas|mostrar comandos|qué sabes hacer)\b", RegexOptions.IgnoreCase))
            {
                return HelpText;
            }

            // Comandos simples
            if (lowered.Contains("hola"))
            {
                var greeting = GetGreeting();

                return UserName != null
                    ? $"{greeting}, {UserName}. Soy {AssistantName}. ¿En qué te ayudo?"
                    : $"{greeting}. Soy {AssistantName}. ¿En qué te ayudo?";
            }


            if (lowered.Contains("hora"))
                return $"Son las {DateTime.Now:HH:mm}.";

            if (lowered.Contains("fecha") || lowered.Contains("hoy") || lowered.Contains("día") || lowered.Contains("dia"))
                return DateTime.Now.ToString("D", CultureInfo.CurrentCulture);

            if (Regex.IsMatch(lowered, @"\b(adiós|adios|hasta luego|nos vemos|chao)\b"))
            {
                return Pick(Goodbyes) + ".";
            }

            if (Regex.IsMatch(lowered, @"\b(chiste|cuenta un chiste|hazme reír|hazme reir)\b"))
            {
                return Pick(Jokes);
            }

            // Extraer números (admite decimales con ',' o '.')
            static double[] ExtractNumbers(string s)
            {
                var list = new System.Collections.Generic.List<double>();
                foreach (Match m in Regex.Matches(s, @"\d+[.,]?\d*"))
                {
                    var token = m.Value.Replace(',', '.');
                    if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                        list.Add(v);
                }
                return list.ToArray();
            }

            // Función auxiliar parseo único
            static bool TryParseFirstNumber(string s, out double value)
            {
                value = 0;
                var m = Regex.Match(s, @"\d+[.,]?\d*");
                if (!m.Success) return false;
                var token = m.Value.Replace(',', '.');
                return double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            }

            // ----------- Cálculos básicos previos --------------
            // Suma
            if (Regex.IsMatch(lowered, @"\b(suma|sumar|añade|agrega|más|calcula)\b"))
            {
                var nums = ExtractNumbers(original);
                if (nums.Length >= 1)
                {
                    double res = 0;
                    foreach (var n in nums) res += n;
                    return $"El resultado es {res.ToString(CultureInfo.CurrentCulture)}.";
                }
                return "No encontré números para sumar.";
            }

            // Resta
            if (Regex.IsMatch(lowered, @"\b(resta|restar|menos)\b"))
            {
                var nums = ExtractNumbers(original);
                if (nums.Length >= 1)
                {
                    double res = nums[0];
                    for (int i = 1; i < nums.Length; i++) res -= nums[i];
                    return $"El resultado es {res.ToString(CultureInfo.CurrentCulture)}.";
                }
                return "No encontré números para restar.";
            }

            // Multiplicación
            if (Regex.IsMatch(lowered, @"\b(multiplica|multiplicar|por|producto|multiplicación)\b"))
            {
                var nums = ExtractNumbers(original);
                if (nums.Length >= 1)
                {
                    double res = 1;
                    foreach (var n in nums) res *= n;
                    return $"El resultado es {res.ToString(CultureInfo.CurrentCulture)}.";
                }
                return "No encontré números para multiplicar.";
            }

            // División
            if (Regex.IsMatch(lowered, @"\b(divide|dividir|entre)\b") &&
                !Regex.IsMatch(lowered, @"\b(aleatorio|azar)\b"))
            {
                var nums = ExtractNumbers(original);
                if (nums.Length >= 2)
                {
                    double res = nums[0];
                    for (int i = 1; i < nums.Length; i++)
                    {
                        if (nums[i] == 0)
                            return "Error: división por cero.";
                        res /= nums[i];
                    }
                    return $"El resultado es {res.ToString(CultureInfo.CurrentCulture)}.";
                }
                return "No encontré suficientes números para dividir (se necesitan al menos 2).";
            }

            // ----------- Porcentaje: "cuánto es 20% de 50" -----------
            var percMatch = Regex.Match(lowered, @"([0-9]+[.,]?[0-9]*)\s*(?:%|por ciento|porciento)\s*(?:de)?\s*([0-9]+[.,]?[0-9]*)");
            if (percMatch.Success)
            {
                var left = percMatch.Groups[1].Value.Replace(',', '.');
                var right = percMatch.Groups[2].Value.Replace(',', '.');
                if (double.TryParse(left, NumberStyles.Any, CultureInfo.InvariantCulture, out var p) &&
                    double.TryParse(right, NumberStyles.Any, CultureInfo.InvariantCulture, out var total))
                {
                    var result = total * (p / 100.0);
                    return $"{p.ToString(CultureInfo.CurrentCulture)}% de {total.ToString(CultureInfo.CurrentCulture)} es {result.ToString(CultureInfo.CurrentCulture)}.";
                }
            }

            // ----------- Conversión de temperatura (mejor detección) -----------
            // Normalizar símbolos comunes
            var normalized = lowered.Replace("°", " ").Replace("º", " ").Replace("grados", " ").Replace("ºc", " c").Replace("°c", " c").Replace("ºf", " f").Replace("°f", " f");

            // Patrón C -> F (captura número seguido de 'c' o 'celsius' y luego 'f' o 'fahrenheit')
            var cToF = Regex.Match(normalized, @"([0-9]+[.,]?[0-9]*)\s*(?:°\s*)?(c|celsius|centigrad(?:o|os|a|as)?)\b.*?\b(f|fahrenheit|farenheit)\b");
            if (cToF.Success)
            {
                var token = cToF.Groups[1].Value.Replace(',', '.');
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var cVal))
                {
                    var f = cVal * 9.0 / 5.0 + 32.0;
                    return $"{cVal.ToString(CultureInfo.CurrentCulture)} °C son {Math.Round(f, 2).ToString(CultureInfo.CurrentCulture)} °F.";
                }
            }

            // Patrón F -> C (captura número seguido de 'f' o 'fahrenheit' y luego 'c' o 'celsius')
            var fToC = Regex.Match(normalized, @"([0-9]+[.,]?[0-9]*)\s*(?:°\s*)?(f|fahrenheit|farenheit)\b.*?\b(c|celsius|centigrad(?:o|os|a|as)?)\b");
            if (fToC.Success)
            {
                var token = fToC.Groups[1].Value.Replace(',', '.');
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var fVal))
                {
                    var c = (fVal - 32.0) * 5.0 / 9.0;
                    return $"{fVal.ToString(CultureInfo.CurrentCulture)} °F son {Math.Round(c, 2).ToString(CultureInfo.CurrentCulture)} °C.";
                }
            }

            // Soportar frases del estilo "¿cuántos Fahrenheit son 20 Celsius?" (numero aparece después de palabra 'son' o similar)
            var reverseCtoF = Regex.Match(normalized, @"\b(c|celsius|centigrad(?:o|os|a|as)?)\b.*?([0-9]+[.,]?[0-9]*).*?\b(f|fahrenheit|farenheit)\b");
            if (reverseCtoF.Success)
            {
                var token = reverseCtoF.Groups[2].Value.Replace(',', '.');
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var cVal))
                {
                    var f = cVal * 9.0 / 5.0 + 32.0;
                    return $"{cVal.ToString(CultureInfo.CurrentCulture)} °C son {Math.Round(f, 2).ToString(CultureInfo.CurrentCulture)} °F.";
                }
            }

            var reverseFtoC = Regex.Match(normalized, @"\b(f|fahrenheit|farenheit)\b.*?([0-9]+[.,]?[0-9]*).*?\b(c|celsius|centigrad(?:o|os|a|as)?)\b");
            if (reverseFtoC.Success)
            {
                var token = reverseFtoC.Groups[2].Value.Replace(',', '.');
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var fVal))
                {
                    var c = (fVal - 32.0) * 5.0 / 9.0;
                    return $"{fVal.ToString(CultureInfo.CurrentCulture)} °F son {Math.Round(c, 2).ToString(CultureInfo.CurrentCulture)} °C.";
                }
            }

            // ----------- Conversión de distancia/longitud -----------
            // kilómetros a millas
            if (Regex.IsMatch(lowered, @"\b(km|kil[oó]metros?|kilometros?)\b") &&
                Regex.IsMatch(lowered, @"\b(mi|millas?)\b") &&
                Regex.IsMatch(lowered, @"\b(km.*mi|kil.*mi)\b"))
            {
                if (TryParseFirstNumber(original, out var val))
                {
                    var miles = val * 0.621371;
                    return $"{val.ToString(CultureInfo.CurrentCulture)} km ≈ {Math.Round(miles, 4).ToString(CultureInfo.CurrentCulture)} mi.";
                }
            }

            // millas a kilómetros
            if (Regex.IsMatch(lowered, @"\b(mi|millas?)\b") &&
                Regex.IsMatch(lowered, @"\b(km|kil[oó]metros?|kilometros?)\b") &&
                Regex.IsMatch(lowered, @"\b(mi.*km)\b"))
            {
                if (TryParseFirstNumber(original, out var val))
                {
                    var km = val / 0.621371;
                    return $"{val.ToString(CultureInfo.CurrentCulture)} mi ≈ {Math.Round(km, 4).ToString(CultureInfo.CurrentCulture)} km.";
                }
            }


            // metros a kilómetros
            if (Regex.IsMatch(lowered, @"\b(metros?|m)\b") &&
                Regex.IsMatch(lowered, @"\b(km|kil[oó]metros?)\b"))
            {
                if (TryParseFirstNumber(original, out var val))
                {
                    var km = val / 1000.0;
                    return $"{val.ToString(CultureInfo.CurrentCulture)} m ≈ {Math.Round(km, 4).ToString(CultureInfo.CurrentCulture)} km.";
                }
            }


            // ----------- Raíz cuadrada y potencias simples -----------
            var raizMatch = Regex.Match(lowered, @"\b(raiz|raíz|sqrt)\b.*?([0-9]+[.,]?[0-9]*)");
            if (raizMatch.Success)
            {
                var token = raizMatch.Groups[2].Value.Replace(',', '.');
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var n))
                {
                    var r = Math.Sqrt(n);
                    return $"La raíz cuadrada de {n.ToString(CultureInfo.CurrentCulture)} es {Math.Round(r, 6).ToString(CultureInfo.CurrentCulture)}.";
                }
            }

            var powMatch = Regex.Match(lowered, @"([0-9]+[.,]?[0-9]*)\s*(?:a la potencia|al cuadrado|al cubo|\^)\s*([0-9]+)");
            if (powMatch.Success)
            {
                if (double.TryParse(powMatch.Groups[1].Value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var bas) &&
                    int.TryParse(powMatch.Groups[2].Value, out var exp))
                {
                    var r = Math.Pow(bas, exp);
                    return $"{bas.ToString(CultureInfo.CurrentCulture)}^{exp} = {r.ToString(CultureInfo.CurrentCulture)}.";
                }
            }

            // ----------- Intentar evaluar expresión matemática directa -----------
            var exprCandidate = Regex.Replace(original, @"[^\d\+\-\*\/\(\)\.,\s%]", string.Empty);
            exprCandidate = exprCandidate.Replace(',', '.').Trim();

            // Si contiene porcentaje en una expresión simple, expandirlo (ej. "50% * 200" -> "(50/100)*200")
            if (exprCandidate.Contains("%"))
            {
                exprCandidate = Regex.Replace(exprCandidate, @"([0-9]+(\.[0-9]+)?)\s*%", "($1/100)");
            }

            if (!string.IsNullOrEmpty(exprCandidate) && Regex.IsMatch(exprCandidate, @"[\d\+\-\*\/\(\)]"))
            {
                try
                {
                    var table = new DataTable();
                    var resultObj = table.Compute(exprCandidate, null);
                    if (resultObj != null && double.TryParse(resultObj.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                    {
                        return $"El resultado es {result.ToString(CultureInfo.CurrentCulture)}.";
                    }
                }
                catch
                {
                    // Ignorar y caer al fallback
                }
            }

            // ----------- Guardar nombre del usuario -----------
            var nameMatch = Regex.Match(lowered, @"\b(me llamo|mi nombre es|soy)\s+([a-záéíóúñ]+)");
            if (nameMatch.Success)
            {
                UserName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nameMatch.Groups[2].Value);
                return $"Encantado de conocerte, {UserName}.";
            }

            if (Regex.IsMatch(lowered, @"\b(mi nombre|cómo me llamo|como me llamo)\b"))
            {
                return UserName != null
                    ? $"Te llamas {UserName}."
                    : "Aún no me has dicho tu nombre.";
            }

            if (Regex.IsMatch(lowered, @"\b(tu nombre|cómo te llamas|como te llamas|quién eres|quien eres)\b"))
            {
                return $"Me llamo {AssistantName}, tu asistente.";
            }

            // Número aleatorio en rango
            var rangeMatch = Regex.Match(
                lowered,
                @"\bentre\s+(?:el\s+)?([0-9]+[.,]?[0-9]*)\s+y\s+(?:el\s+)?([0-9]+[.,]?[0-9]*)\b"
            );

            if (rangeMatch.Success)
            {
                var min = int.Parse(rangeMatch.Groups[1].Value);
                var max = int.Parse(rangeMatch.Groups[2].Value);

                if (min > max) (min, max) = (max, min);

                var n = Random.Shared.Next(min, max + 1);
                return $"Número aleatorio entre {min} y {max}: {n}.";
            }

            // Número aleatorio
            if (Regex.IsMatch(lowered, @"\b(número aleatorio|numero aleatorio|dame un número|dame un numero)\b"))
            {
                int n = Random.Shared.Next(1, 101);
                return $"Aquí tienes un número aleatorio: {n}.";
            }

            // fallback
            return "No he reconocido el comando.";
        }
    }
}