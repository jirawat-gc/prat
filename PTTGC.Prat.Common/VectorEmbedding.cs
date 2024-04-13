using Newtonsoft.Json;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.Intrinsics;

namespace PTTGC.Prat.Common;

public class VectorEmbedding
{
    public const int VECTOR_LENGTH = 768;

    private double[] _Vector = new double[VECTOR_LENGTH];
    private string _Encoded = string.Empty;

    private double? _Magnitude;

    private void Decompress()
    {
        if (_Vector != null)
        {
            return;
        }

        try
        {
            _Vector = VectorEmbedding.Decode(_Encoded);
            _Magnitude = VectorEmbedding.Magnitude(_Vector);
        }
        catch (Exception)
        {
            _Vector = new double[VECTOR_LENGTH];
        }

    }

    public static double Magnitude(double[] vector)
    {
        return Math.Sqrt(vector.Select(x => x * x).Sum());
    }

    /// <summary>
    /// Compute Cosine Similarity
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static double CosineSimilarity(double[] vectorA, double[] vectorB, double? magnitudeA, double? magnitudeB)
    {
        if (vectorA.Length != vectorB.Length)
        {
            throw new ArgumentException("Vectors must have the same dimension.");
        }

        double dotProduct = vectorA.Zip(vectorB, (a, b) => a * b).Sum();
        magnitudeA = magnitudeA ?? Magnitude(vectorA);
        magnitudeB = magnitudeB ?? Magnitude(vectorB);

        if (magnitudeA == 0 || magnitudeB == 0)
        {
            return 0; // Avoid division by zero.
        }

        return dotProduct / (magnitudeA.Value * magnitudeB.Value);
    }

    /// <summary>
    /// Encodes embeddings into compressed binary format encoded in base64
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string Encode(double[] value)
    {
        if (value.Length != VECTOR_LENGTH)
        {
            throw new InvalidOperationException($"Vector is not valid, expect {VECTOR_LENGTH} dimension.");
        }

        using var ms = new MemoryStream();
        using var gz = new GZipStream(ms, CompressionLevel.SmallestSize, true);
        using var bw = new BinaryWriter(gz);

        for (int i = 0; i < VECTOR_LENGTH; i++)
        {
            bw.Write(value[i]);
        }

        bw.Flush();
        gz.Flush();

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Decodes base64 encoded embeddings back to number format
    /// </summary>
    /// <param name="base64"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static double[] Decode(string base64)
    {
        using var ms = new MemoryStream(Convert.FromBase64String(base64));
        using var gz = new GZipStream(ms, CompressionMode.Decompress);
        using var br = new BinaryReader(gz);
        ms.Position = 0;

        var vector = new double[VECTOR_LENGTH];

        for (int i = 0; i < VECTOR_LENGTH; i++)
        {
            try
            {
                vector[i] = br.ReadDouble();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("base64 value is not a valid vector embedding");
            }
        }

        return vector;
    }

    #region Implcit Conversions

    public static implicit operator double[](VectorEmbedding vector)
    {
        return vector._Vector;
    }

    public static implicit operator VectorEmbedding(double[] array)
    {
        if (array.Length != VECTOR_LENGTH)
        {
            throw new InvalidOperationException();
        }

        return new VectorEmbedding()
        {
            _Vector = array,
            _Magnitude = VectorEmbedding.Magnitude(array)
        };
    }

    public static implicit operator string(VectorEmbedding vector)
    {
        return VectorEmbedding.Encode( vector._Vector );
    }

    public static implicit operator VectorEmbedding(string base64)
    {
        var array = VectorEmbedding.Decode(base64);

        if (array.Length != VECTOR_LENGTH)
        {
            throw new InvalidOperationException();
        }

        return new VectorEmbedding()
        {
            _Vector = array,
            _Magnitude = VectorEmbedding.Magnitude(array)
        };
    }

    public override string ToString()
    {
        return VectorEmbedding.Encode(_Vector);
    }

    public override int GetHashCode()
    {
        return VectorEmbedding.Encode(_Vector).GetHashCode();
    }

    #endregion

    public class VectorEmbeddingConverter : JsonConverter<VectorEmbedding>
    {
        public override void WriteJson(JsonWriter writer, VectorEmbedding value, JsonSerializer serializer)
        {
            writer.WriteValue((string)value);
        }

        public override VectorEmbedding ReadJson(JsonReader reader, Type objectType, VectorEmbedding existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string base64 = (string)reader.Value;

            return (VectorEmbedding)base64;
        }
    }
}
