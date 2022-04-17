using System;
using NUnit.Framework;
using static Tiny.CommandLine.Tests.Helper;

namespace Tiny.CommandLine.Tests
{
    [TestFixture]
    public class OptionsTypesTests
    {
        [OneTimeSetUp]
        protected void SetUp()
        {
            OverrideOutput();
            OverrideExit();
        }

        [TestCase("-v \"test\"", ExpectedResult = "test")]
        [TestCase("--value=\"test\"", ExpectedResult = "test")]
        public string String_values_should_be_parsed(string cmd) => ParseOption<string>(cmd);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestCase("-v", ExpectedResult = true)]
        [TestCase("--value=false", ExpectedResult = false)]
        public bool Bool_values_should_be_parsed(string cmd) => ParseOption<bool>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v", ExpectedResult = true)]
        [TestCase("--value=false", ExpectedResult = false)]
        public bool? Nullable_Bool_values_should_be_parsed(string cmd) => ParseOption<bool?>(cmd);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestCase("-v a", ExpectedResult = 'a')]
        [TestCase("--value c", ExpectedResult = 'c')]
        public char Char_values_should_be_parsed(string cmd) => ParseOption<char>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v a", ExpectedResult = 'a')]
        [TestCase("--value c", ExpectedResult = 'c')]
        public char? Nullable_Char_values_should_be_parsed(string cmd) => ParseOption<char?>(cmd);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestCase("-v 9223372036854775807", ExpectedResult = long.MaxValue)]
        [TestCase("--value -9223372036854775808", ExpectedResult = long.MinValue)]
        public long Long_values_should_be_parsed(string cmd) => ParseOption<long>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v 9223372036854775807", ExpectedResult = long.MaxValue)]
        [TestCase("--value -9223372036854775808", ExpectedResult = long.MinValue)]
        public long? Nullable_Long_values_should_be_parsed(string cmd) => ParseOption<long?>(cmd);


        [TestCase("-v 18446744073709551615", ExpectedResult = ulong.MaxValue)]
        [TestCase("--value 0", ExpectedResult = ulong.MinValue)]
        public ulong ULong_values_should_be_parsed(string cmd) => ParseOption<ulong>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v 18446744073709551615", ExpectedResult = ulong.MaxValue)]
        [TestCase("--value 0", ExpectedResult = ulong.MinValue)]
        public ulong? Nullable_ULong_values_should_be_parsed(string cmd) => ParseOption<ulong?>(cmd);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestCase("-v 2147483647", ExpectedResult = int.MaxValue)]
        [TestCase("--value -2147483648", ExpectedResult = int.MinValue)]
        public int Int_values_should_be_parsed(string cmd) => ParseOption<int>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v 2147483647", ExpectedResult = int.MaxValue)]
        [TestCase("--value -2147483648", ExpectedResult = int.MinValue)]
        public int? Nullable_Int_values_should_be_parsed(string cmd) => ParseOption<int?>(cmd);


        [TestCase("-v 4294967295", ExpectedResult = uint.MaxValue)]
        [TestCase("--value 0", ExpectedResult = uint.MinValue)]
        public uint UInt_values_should_be_parsed(string cmd) => ParseOption<uint>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v 4294967295", ExpectedResult = uint.MaxValue)]
        [TestCase("--value 0", ExpectedResult = uint.MinValue)]
        public uint? Nullable_UInt_values_should_be_parsed(string cmd) => ParseOption<uint?>(cmd);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestCase("-v 32767", ExpectedResult = short.MaxValue)]
        [TestCase("--value -32768", ExpectedResult = short.MinValue)]
        public short Short_values_should_be_parsed(string cmd) => ParseOption<short>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v 32767", ExpectedResult = short.MaxValue)]
        [TestCase("--value -32768", ExpectedResult = short.MinValue)]
        public short? Nullable_Short_values_should_be_parsed(string cmd) => ParseOption<short?>(cmd);


        [TestCase("-v 65535", ExpectedResult = ushort.MaxValue)]
        [TestCase("--value 0", ExpectedResult = ushort.MinValue)]
        public ushort UShort_values_should_be_parsed(string cmd) => ParseOption<ushort>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v 65535", ExpectedResult = ushort.MaxValue)]
        [TestCase("--value 0", ExpectedResult = ushort.MinValue)]
        public ushort? Nullable_UShort_values_should_be_parsed(string cmd) => ParseOption<ushort?>(cmd);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestCase("-v 127", ExpectedResult = sbyte.MaxValue)]
        [TestCase("--value -128", ExpectedResult = sbyte.MinValue)]
        public sbyte SByte_values_should_be_parsed(string cmd) => ParseOption<sbyte>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v 127", ExpectedResult = sbyte.MaxValue)]
        [TestCase("--value -128", ExpectedResult = sbyte.MinValue)]
        public sbyte? Nullable_SByte_values_should_be_parsed(string cmd) => ParseOption<sbyte?>(cmd);


        [TestCase("-v 255", ExpectedResult = byte.MaxValue)]
        [TestCase("--value 0", ExpectedResult = byte.MinValue)]
        public byte Byte_values_should_be_parsed(string cmd) => ParseOption<byte>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v 255", ExpectedResult = byte.MaxValue)]
        [TestCase("--value 0", ExpectedResult = byte.MinValue)]
        public byte? Nullable_Byte_values_should_be_parsed(string cmd) => ParseOption<byte?>(cmd);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestCase("-v 123.456", ExpectedResult = 123.456f)]
        [TestCase("--value -123.456", ExpectedResult = -123.456f)]
        public float Float_values_should_be_parsed(string cmd) => ParseOption<float>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v 123.456", ExpectedResult = 123.456f)]
        [TestCase("--value -123.456", ExpectedResult = -123.456f)]
        public float? Nullable_Float_values_should_be_parsed(string cmd) => ParseOption<float?>(cmd);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestCase("-v 123456.789012", ExpectedResult = 123456.789012)]
        [TestCase("--value -123456.789012", ExpectedResult = -123456.789012)]
        public double Double_values_should_be_parsed(string cmd) => ParseOption<double>(cmd);

        [TestCase("", ExpectedResult = null)]
        [TestCase("-v 123456.789012", ExpectedResult = 123456.789012)]
        [TestCase("--value -123456.789012", ExpectedResult = -123456.789012)]
        public double? Nullable_Double_values_should_be_parsed(string cmd) => ParseOption<double?>(cmd);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestCase("-v 0.1234567891234567891234567")]
        public void Decimal_float_value_should_be_parsed(string cmd) => Assert.AreEqual(ParseOption<decimal>(cmd), 0.1234567891234567891234567M);

        [TestCase("-v 79228162514264337593543950335")]
        public void Decimal_max_value_should_be_parsed(string cmd) => Assert.AreEqual(ParseOption<decimal>(cmd), decimal.MaxValue);

        [TestCase("-v -79228162514264337593543950335")]
        public void Decimal_min_value_should_be_parsed(string cmd) => Assert.AreEqual(ParseOption<decimal>(cmd), decimal.MinValue);


        [TestCase("")]
        public void Nullable_Decimal_null_value_should_be_parsed(string cmd) => Assert.AreEqual(ParseOption<decimal?>(cmd), null);

        [TestCase("--value 0.1234567891234567891234567")]
        public void Nullable_Decimal_float_value_should_be_parsed(string cmd) => Assert.AreEqual(ParseOption<decimal?>(cmd), 0.1234567891234567891234567M);

        [TestCase("--value 79228162514264337593543950335")]
        public void Nullable_Decimal_max_value_should_be_parsed(string cmd) => Assert.AreEqual(ParseOption<decimal?>(cmd), decimal.MaxValue);

        [TestCase("--value -79228162514264337593543950335")]
        public void Nullable_Decimal_min_value_should_be_parsed(string cmd) => Assert.AreEqual(ParseOption<decimal?>(cmd), decimal.MinValue);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        [TestCase("-v 01.20.1997")]
        public void DateTime_values_should_be_parsed(string cmd) => Assert.AreEqual(ParseOption<DateTime>(cmd), new DateTime(1997, 01, 20));

        [TestCase("")]
        public void Nullable_DateTime_null_value_should_be_parsed(string cmd) => Assert.AreEqual(ParseOption<DateTime?>(cmd), null);

        [TestCase("-v 02.25.2015")]
        public void Nullable_DateTime_values_should_be_parsed(string cmd) => Assert.AreEqual(ParseOption<DateTime?>(cmd), new DateTime(2015, 02, 25));

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        public class UnknownType { }

        [TestCase("-v custom")]
        public void Unknown_types_should_throw_exception(string cmd) => Assert.Throws<NotSupportedException>(() => ParseOption<UnknownType>(cmd));

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        static T ParseOption<T>(string cmd)
        {
            T result = default;
            Run(cmd, s => s.Option('v', "value", out result));

            return result;
        }
    }
}