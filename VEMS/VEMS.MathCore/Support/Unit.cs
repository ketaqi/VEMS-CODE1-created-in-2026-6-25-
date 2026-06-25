namespace VEMS.MathCore
{
    /// <summary>
    /// quantity dimension / physical dimension
    /// </summary>
    public class Dimension : IEquatable<Dimension>
    {
        #region properties

        /// <summary>
        /// dimension: time
        /// </summary>
        public int T { get; set; }

        /// <summary>
        /// dimension: length
        /// </summary>
        public int L { get; set; }

        /// <summary>
        /// dimension: mass
        /// </summary>
        public int M { get; set; }

        /// <summary>
        /// dimension: electric current
        /// </summary>
        public int I { get; set; }

        /// <summary>
        /// dimension: absolute temperature
        /// </summary>
        public int Theta { get; set; }

        /// <summary>
        /// dimension: amount of substance
        /// </summary>
        public int N { get; set; }

        /// <summary>
        /// dimension: luminous intensity
        /// </summary>
        public int J { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a quantity dimension
        /// </summary>
        /// <param name="t"> exponent of the time dimension </param>
        /// <param name="l"> exponent of the length dimention </param>
        /// <param name="m"> exponent of the mass dimension </param>
        /// <param name="i"> exponent of the electric current dimension </param>
        /// <param name="theta"> exponent of the absolute temperature dimension </param>
        /// <param name="n"> exponent of the amount of substance dimension </param>
        /// <param name="j"> exponent of the luminous intensity dimension </param>
        public Dimension(int t = 0, int l = 0,
            int m = 0, int i = 0, int theta = 0,
            int n = 0, int j = 0)
        {
            T = t;
            L = l;
            M = m;
            I = i;
            Theta = theta;
            N = n;
            J = j;
        }

        /// <summary>
        /// constructs a quantity dimension
        /// by copying from another
        /// </summary>
        /// <param name="other"> source dimension </param>
        public Dimension(Dimension other)
        {
            T = other.T;
            L = other.L;
            M = other.M;
            I = other.I;
            Theta = other.Theta;
            N = other.N;
            J = other.J;
        }

        #endregion
        #region methods

        /// <summary>
        /// checks whether it is the same
        /// </summary>
        /// <param name="other"> another dimension to compare </param>
        /// <returns> whether it is the same </returns>
        public bool Equals(Dimension? other)
        {
            // null case handling
            if (other == null) { return false; }
            // checks each dimension
            if (other.T != T) { return false; }
            if (other.L != L) { return false; }
            if (other.M != M) { return false; }
            if (other.I != I) { return false; }
            if (other.Theta != Theta) { return false; }
            if (other.N != N) { return false; }
            if (other.J != J) { return false; }
            // if all are the same
            return true;
        }

        /// <summary>
        /// converts dimension to string
        /// </summary>
        /// <returns> dimension in string format </returns>
        public string ConvertToString()
        {
            string dims = string.Empty;
            if (T != 0) { dims += $"T^{T}"; }
            if (L != 0) { dims += $"L^{L}"; }
            if (M != 0) { dims += $"M^{M}"; }
            if (I != 0) { dims += $"I^{I}"; }
            if (Theta != 0) { dims += $"Theta^{Theta}"; }
            if (N != 0) { dims += $"N^{N}"; }
            if (J != 0) { dims += $"J^{J}"; }
            return dims;
        }

        #endregion
        #region operators

        /// <summary>
        /// performs multiplication between two dimensions
        /// </summary>
        /// <param name="d1"> dimension #1 </param>
        /// <param name="d2"> dimension #2 </param>
        /// <returns> result dimension after multiplication </returns>
        public static Dimension operator *(Dimension d1, Dimension d2)
            => new(t: d1.T + d2.T,
                l: d1.L + d2.L,
                m: d1.M + d2.M,
                i: d1.I + d2.I,
                theta: d1.Theta + d2.Theta,
                n: d1.N + d2.N,
                j: d1.J + d2.J);

        /// <summary>
        /// performs division between two dimensions
        /// </summary>
        /// <param name="d1"> dimension #1 </param>
        /// <param name="d2"> dimension #2 </param>
        /// <returns> result dimension after divisoin (d1/d2) </returns>
        public static Dimension operator /(Dimension d1, Dimension d2)
            => new(t: d1.T - d2.T,
                l: d1.L - d2.L,
                m: d1.M - d2.M,
                i: d1.I - d2.I,
                theta: d1.Theta - d2.Theta,
                n: d1.N - d2.N,
                j: d1.J - d2.J);

        /// <summary>
        /// performs n-power of a dimension
        /// </summary>
        /// <param name="d"> input dimension </param>
        /// <param name="n"> integer exponent </param>
        /// <returns></returns>
        public static Dimension operator ^(Dimension d, int n)
            => new(t: d.T * n,
                l: d.L * n,
                m: d.M * n,
                i: d.I * n,
                theta: d.Theta * n,
                n: d.N * n,
                j: d.J * n);

        #endregion
        #region static fields

        /// <summary>
        /// dimension: one
        /// </summary>
        public static Dimension One
            => new();

        /// <summary>
        /// dimension: time
        /// </summary>
        public static Dimension Time
            => new(t: 1);

        /// <summary>
        /// dimension: length
        /// </summary>
        public static Dimension Length
            => new(l: 1);

        /// <summary>
        /// dimension: mass
        /// </summary>
        public static Dimension Mass
            => new(m: 1);


        /// <summary>
        /// dimension: (electic) current
        /// </summary>
        public static Dimension Current
            => new(i: 1);

        /// <summary>
        /// dimension: (absolute) temperature
        /// </summary>
        public static Dimension Temperature
            => new(theta: 1);

        /// <summary>
        /// dimension: amount of substance
        /// </summary>
        public static Dimension AmountOfSubstance
            => new(n: 1);

        /// <summary>
        /// dimension: luminous intensity
        /// </summary>
        public static Dimension LuminousIntensity
            => new(j: 1);

        #endregion
    }

    /// <summary>
    /// unit of measure
    /// </summary>
    public class Unit
    {
        #region properties

        /// <summary>
        /// dimension of the unit
        /// </summary>
        public Dimension Dimension { get; set; }

        /// <summary>
        /// scaling w.r.t. SI unit
        /// </summary>
        public double Scaling { get; set; } = 1.0;

        /// <summary>
        /// physical quantity of the unit
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// name of the unit
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// symbol of the unit
        /// </summary>
        public string Symbol { get; set; }

        #endregion
        #region constructors

        /// <summary>
        /// constructs a unit
        /// </summary>
        /// <param name="dim"> dimension of the unit </param>
        /// <param name="name"> name of the unit </param>
        /// <param name="symbol"> symbol of the unit </param>
        /// <param name="quantity"> physical quantity of the unit </param>
        /// <param name="scaling"> scaling w.r.t. SI unit </param>
        internal Unit(Dimension dim,
            string name,
            string symbol,
            string quantity,
            double scaling = 1.0)
        {
            Dimension = dim;
            Name = name;
            Symbol = symbol;
            Quantity = quantity;
            Scaling = scaling;
        }

        #endregion
        #region methods

        /// <summary>
        /// converts dimension to string
        /// </summary>
        /// <returns> dimension in string format </returns>
        public string DimensionToString()
            => Dimension.ConvertToString();

        /// <summary>
        /// creates a new unit by combining a prefix and a SI unit
        /// </summary>
        /// <param name="prefix"> prefix of the new unit </param>
        /// <param name="unit"> SI unit as the basis </param>
        /// <returns> resulting new unit </returns>
        public static Unit Create(SIPrefix prefix, SIUnit unit)
            => new(dim: new Dimension(unit.Dimension),
                name: prefix.Name + unit.Name,
                symbol: prefix.Symbol + unit.Symbol,
                quantity: unit.Quantity,
                scaling: prefix.Scaling);

        #endregion
    }

    /// <summary>
    /// SI unit
    /// </summary>
    public class SIUnit : Unit
    {
        #region constructor
        internal SIUnit(Dimension dim,
            string name, string symbol, string quantity)
            : base(dim, name, symbol, quantity)
        { }
        #endregion
        #region sub-classes

        /// <summary>
        /// SI base unit
        /// </summary>
        public class Base : SIUnit
        {
            #region constructor
            internal Base(Dimension dim,
                string name, string symbol, string quantity)
                : base(dim, name, symbol, quantity)
            { }
            #endregion
            #region static fields (7x)

            /// <summary>
            /// second: base unit of time
            /// </summary>
            public static Base Second
                => new(dim: Dimension.Time,
                    name: "second", symbol: "s",
                    quantity: "time");

            /// <summary>
            /// meter: base unit of length
            /// </summary>
            public static Base Metre
                => new(dim: Dimension.Length,
                    name: "metre", symbol: "m",
                    quantity: "length");

            /// <summary>
            /// kilogram: base unit of mass
            /// </summary>
            public static Base Kilogram
                => new(dim: Dimension.Mass,
                    name: "kilogram", symbol: "kg",
                    quantity: "mass");

            /// <summary>
            /// ampere: base unit of electric current
            /// </summary>
            public static Base Ampere
                => new(dim: Dimension.Current,
                    name: "ampere", symbol: "A",
                    quantity: "electric current");

            /// <summary>
            /// kelvin: base unit of thermodynamic temperature
            /// </summary>
            public static Base Kelvin
                => new(dim: Dimension.Temperature,
                    name: "kelvin", symbol: "K",
                    quantity: "thermodynamic temperature");

            /// <summary>
            /// mole: base unit of the amount of substance
            /// </summary>
            public static Base Mole
                => new(dim: Dimension.AmountOfSubstance,
                    name: "mole", symbol: "mol",
                    quantity: "amount of substance");

            /// <summary>
            /// candela: base unit of luminous intensity
            /// </summary>
            public static Base Candela
                => new(dim: Dimension.LuminousIntensity,
                    name: "candela", symbol: "cd",
                    quantity: "luminous intensity");

            #endregion
        }

        /// <summary>
        /// SI derived unit
        /// </summary>
        public class Derived : SIUnit
        {
            #region constructor
            internal Derived(Dimension dim,
                string name, string symbol, string quantity)
                : base(dim, name, symbol, quantity)
            { }
            #endregion
            #region static fields (22x)

            /// <summary>
            /// radian: unit of plane angle
            /// </summary>
            public static Derived Radian
                => new(dim: Dimension.One,
                    name: "radian", symbol: "rad",
                    quantity: "plane angle");

            /// <summary>
            /// steradian: unit of solid angle
            /// </summary>
            public static Derived Steradian
                => new(dim: Dimension.One,
                    name: "steradian", symbol: "sr",
                    quantity: "solid angle");

            /// <summary>
            /// hertz: unit of frequency
            /// </summary>
            public static Derived Hertz
                => new(dim: new(t: -1),
                    name: "hertz", symbol: "Hz",
                    quantity: "frequency");

            /// <summary>
            /// newton: unit of force
            /// </summary>
            public static Derived Newton
                => new(dim: new(t: -2, l: 1, m: 1),
                    name: "newton", symbol: "N",
                    quantity: "force");

            /// <summary>
            /// pascal: unit of pressure, stress
            /// </summary>
            public static Derived Pascal
                => new(dim: new(t: -2, l: -1, m: 1),
                    name: "pascal", symbol: "Pa",
                    quantity: "pressure/stress");

            /// <summary>
            /// joule: unit of energy
            /// </summary>
            public static Derived Joule
                => new(dim: new(t: -2, l: 2, m: 1),
                    name: "joule", symbol: "J",
                    quantity: "energy/work/amount of heat");

            /// <summary>
            /// watt: unit of power
            /// </summary>
            public static Derived Watt
                => new(dim: new(t: -3, l: 2, m: 1),
                    name: "watt", symbol: "W",
                    quantity: "power/radiant flux");

            /// <summary>
            /// coulomb: unit of electric charge
            /// </summary>
            public static Derived Coulomb
                => new(dim: new(t: 1, i: 1),
                    name: "coulomb", symbol: "C",
                    quantity: "electric charge");

            /// <summary>
            /// volt: unit of electric potential
            /// </summary>
            public static Derived Volt
                => new(dim: new(t: -3, l: 2, m: 1, i: -1),
                    name: "volt", symbol: "V",
                    quantity: "electric potential");

            /// <summary>
            /// farad: unit of capacitance
            /// </summary>
            public static Derived Farad
                => new(dim: new(t: 4, l: -2, m: -1, i: 2),
                    name: "farad", symbol: "F",
                    quantity: "capacitance");

            /// <summary>
            /// ohm: unit of resistance
            /// </summary>
            public static Derived Ohm
                => new(dim: new(t: -3, l: 2, m: 1, i: -2),
                    name: "ohm", symbol: "Ω",
                    quantity: "resistance");

            /// <summary>
            /// siemens: unit of electrical conductance
            /// </summary>
            public static Derived Siemens
                => new(dim: new(t: 3, l: -2, m: -1, i: 2),
                    name: "siemens", symbol: "S",
                    quantity: "electrical conductance");

            /// <summary>
            /// weber: unit of magnetic flux
            /// </summary>
            public static Derived Weber
                => new(dim: new(t: -2, l: 2, m: 1, i: -1),
                    name: "weber", symbol: "Wb",
                    quantity: "magnetic flux");

            /// <summary>
            /// tesla: unit of magnetic flux density
            /// </summary>
            public static Derived Tesla
                => new(dim: new(t: -2, m: 1, i: -1),
                    name: "tesla", symbol: "T",
                    quantity: "magnetic flux density");

            /// <summary>
            /// henry: unit of inductance
            /// </summary>
            public static Derived Henry
                => new(dim: new(t: -2, l: 2, m: 1, i: -2),
                    name: "henry", symbol: "H",
                    quantity: "inductance");

            /// <summary>
            /// degree Celsius: unit of temperature
            /// </summary>
            public static Derived DegreeCelsius
                => new(dim: new(theta: 1),
                    name: "degree Celsius", symbol: "°C",
                    quantity: "temperature");

            /// <summary>
            /// lumen: unit of luminous flux
            /// </summary>
            public static Derived Lumen
                => new(dim: new(j: 1),
                    name: "lumen", symbol: "lm",
                    quantity: "luminous flux");

            /// <summary>
            /// lux: unit of illuminance
            /// </summary>
            public static Derived Lux
                => new(dim: new(l: -2, j: 1),
                    name: "lux", symbol: "lx",
                    quantity: "illuminance");

            /// <summary>
            /// becquerel: unit of radioactivity
            /// [activity referred to a radionuclide]
            /// </summary>
            public static Derived Becquerel
                => new(dim: new(t: -1),
                    name: "becquerel", symbol: "Bq",
                    quantity: "radioactivity");

            /// <summary>
            /// gray: unit of absorbed dose
            /// </summary>
            public static Derived Gray
                => new(dim: new(t: -2, l: 2),
                    name: "gray", symbol: "Gy",
                    quantity: "absorbed does");

            /// <summary>
            /// sievert: unit of equivalent dose
            /// </summary>
            public static Derived Sievert
                => new(dim: new(t: -2, l: 2),
                    name: "sievert", symbol: "Sv",
                    quantity: "equivalent dose");

            /// <summary>
            /// katal: unit of catalytic activity
            /// </summary>
            public static Derived Katal
                => new(dim: new(t: -1, n: 1),
                    name: "katal", symbol: "kat",
                    quantity: "catalytic activity");

            #endregion
        }

        /// <summary>
        /// SI common unit
        /// </summary>
        public class Common : SIUnit
        {
            #region constructor
            internal Common(Dimension dim,
                string name, string symbol, string quantity)
                : base(dim, name, symbol, quantity)
            { }
            #endregion
            #region static fields

            /// <summary>
            /// pascale-second: unit of dynamic viscosity
            /// </summary>
            public static Common PascalSecond
                => new(dim: new(t: -1, l: -1, m: 1),
                    name: "pascal-second", symbol: "Pa·s",
                    quantity: "dynamic viscosity");

            /// <summary>
            /// newton-metre: unit of moment of force
            /// </summary>
            public static Common NewtonMetre
                => new(dim: new(t: -2, l: 2, m: 1),
                    name: "newton-metre", symbol: "N·m",
                    quantity: "moment of force");

            /// <summary>
            /// newton per metre: unit of surface tension
            /// </summary>
            public static Common NewtonPerMetre
                => new(dim: new(t: -2, m: 1),
                    name: "newton per metre", symbol: "N/m",
                    quantity: "surface tension");

            /// <summary>
            /// radian per second: unit of angular velocity / angular frequency
            /// </summary>
            public static Common RadianPerSecond
                => new(dim: new(t: -1),
                    name: "radian per second", symbol: "rad/s",
                    quantity: "angular velocity / angular frequency");

            /// <summary>
            /// radian per second squared: unit of angular acceleration
            /// </summary>
            public static Common RadianPerSecondSquared
                => new(dim: new(t: -2),
                    name: "radian per second squared", symbol: "rad/s^2",
                    quantity: "angular acceleration");

            /// <summary>
            /// watt per square metre: unit of heat flux density / irradiance
            /// </summary>
            public static Common WattPerSquareMetre
                => new(dim: new(t: -3, m: 1),
                    name: "watt per square metre", symbol: "W/m^2",
                    quantity: "heat flux density / irradiance");

            /// <summary>
            /// joule per kelvin: unit of entropy / heat capacity
            /// </summary>
            public static Common JoulePerKelvin
                => new(dim: new(t: -2, l: 2, m: 1, theta: -1),
                    name: "joule per kelvin", symbol: "J/K",
                    quantity: "entropy, heat capacity");

            /// <summary>
            /// joule per kilogram-kelvin: unit of specific heat capacity
            /// </summary>
            public static Common JoulePerKilogramKelvin
                => new(dim: new(t: -2, l: 2, theta: -1),
                    name: "joule per kilogram-kelvin", symbol: "J/(kg·K)",
                    quantity: "specific heat capacity / specific entropy");

            /// <summary>
            /// joule per kilogram: unit of specific energy
            /// </summary>
            public static Common JoulePerKilogram
                => new(dim: new(t: -2, l: 2),
                    name: "joule per kilogram", symbol: "J/kg",
                    quantity: "specific energy");

            /// <summary>
            /// watt per metre-kelvin: unit of thermal conductivity
            /// </summary>
            public static Common WattPerMetreKelvin
                => new(dim: new(t: -3, l: 1, m: 1, theta: -1),
                    name: "watt per metre kelvin", symbol: "W/(m·K)",
                    quantity: "thermal conductivity");

            /// <summary>
            /// joule per cubic metre: unit of energy density
            /// </summary>
            public static Common JoulePerCubicMetre
                => new(dim: new(t: -2, l: -1, m: 1),
                    name: "joule per cubic metre", symbol: "J/m^3",
                    quantity: "energy density");

            /// <summary>
            /// volt per metre: unit of electric field strength
            /// </summary>
            public static Common VoltPerMetre
                => new(dim: new(t: -3, l: 1, m: 1, i: -1),
                    name: "volt per metre", symbol: "V/m",
                    quantity: "electric field strength");

            /// <summary>
            /// coulomb per cubic metre: unit of electric charge density
            /// </summary>
            public static Common CoulombPerCubicMetre
                => new(dim: new(t: 1, l: -3, i: 1),
                    name: "coulomb per cubic metre", symbol: "C/m^3",
                    quantity: "electric charge density");

            /// <summary>
            /// coulomb per square metre: unit of surface charge density
            /// </summary>
            public static Common CoulombPerSquareMetre
                => new(dim: new(t: 1, l: -2, i: 1),
                    name: "coulomb per square metre", symbol: "C/m^2",
                    quantity: "surface charge density");

            /// <summary>
            /// farad per metre: unit of permittivity
            /// </summary>
            public static Common FaradPerMetre
                => new(dim: new(t: 4, l: -3, m: -1, i: 2),
                    name: "farad per metre", symbol: "F/m",
                    quantity: "permittivity");

            /// <summary>
            /// henry per metre: unit of permeability
            /// </summary>
            public static Common HenryPerMetre
                => new(dim: new(t: -2, l: -1, m: 1, i: -2),
                    name: "henry per metre", symbol: "H/m",
                    quantity: "permeability");

            /// <summary>
            /// joule per metre: unit of molar energy
            /// </summary>
            public static Common JoulePerMetre
                => new(dim: new(t: -2, l: 2, m: 1, n: -1),
                    name: "joule per metre", symbol: "J/m",
                    quantity: "molar energy");

            /// <summary>
            /// joule per mole-kelvin: unit of molar entropy / molar heat capacity
            /// </summary>
            public static Common JoulePerMoleKelvin
                => new(dim: new(t: -2, l: 2, m: 1, theta: -1, n: -1),
                    name: "joule per mole-kelvin", symbol: "J/(mol·K)",
                    quantity: "molar entropy / molar heat capacity");

            /// <summary>
            /// coulomb per kilogram: unit of exposure
            /// </summary>
            public static Common CoulombPerKilogram
                => new(dim: new(t: 1, m: -1, i: 1),
                    name: "coulomb per kilogram", symbol: "C/kg",
                    quantity: "exposure");

            /// <summary>
            /// gray per second: unit of absorbed dose rate
            /// </summary>
            public static Common GrayPerSecond
                => new(dim: new(t: -3, l: 2),
                    name: "gray per second", symbol: "Gy/s",
                    quantity: "absorbed dose rate");

            /// <summary>
            /// watt per steradian: unit of radiant intensity
            /// </summary>
            public static Common WattPerSteradian
                => new(dim: new(t: -3, l: 2, m: 1),
                    name: "watt per steradian", symbol: "W/sr",
                    quantity: "radiant intensity");

            /// <summary>
            /// watt per square metre-steradian: unit of radiance
            /// </summary>
            public static Common WattPerSquareMetreSteradian
                => new(dim: new(t: -3, m: 1),
                    name: "watt per square metre steradian", symbol: "W/(m^2·sr)",
                    quantity: "radiance");

            /// <summary>
            /// katal per cubic metre: unit of catalytic activity concentration
            /// </summary>
            public static Common KatalPerCubicMetre
                => new(dim: new(t: -1, l: -3, n: 1),
                    name: "katal per cubic metre", symbol: "kat/m^3",
                    quantity: "catalytic activity concentration");

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// SI prefixes
    /// </summary>
    public class SIPrefix
    {
        #region properties

        /// <summary>
        /// scaling of the prefix
        /// </summary>
        public double Scaling { get; set; }

        /// <summary>
        /// name of the prefix
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// symbol of the prefix
        /// </summary>
        public string Symbol { get; set; }

        #endregion
        #region constructor
        internal SIPrefix(double scaling,
            string name, string symbol)
        {
            Scaling = scaling;
            Name = name;
            Symbol = symbol;
        }
        #endregion
        #region static fields

        /// <summary>
        /// yotta: 1E24
        /// </summary>
        public static SIPrefix Yotta
            => new(1E24, "yotta", "Y");

        /// <summary>
        /// zetta: 1E21
        /// </summary>
        public static SIPrefix Zetta
            => new(1E21, "zetta", "Z");

        /// <summary>
        /// exa: 1E18
        /// </summary>
        public static SIPrefix Exa
            => new(1E18, "exa", "E");

        /// <summary>
        /// peta: 1E15
        /// </summary>
        public static SIPrefix Peta
            => new(1E15, "peta", "P");

        /// <summary>
        /// tera: 1E12
        /// </summary>
        public static SIPrefix Tera
            => new(1E12, "tera", "T");

        /// <summary>
        /// giga: 1E9
        /// </summary>
        public static SIPrefix Giga
            => new(1E9, "giga", "G");

        /// <summary>
        /// mega: 1E6
        /// </summary>
        public static SIPrefix Mega
            => new(1E6, "mega", "M");

        /// <summary>
        /// kilo: 1E3
        /// </summary>
        public static SIPrefix Kilo
            => new(1E3, "kilo", "k");

        /// <summary>
        /// hecto: 1E2
        /// </summary>
        public static SIPrefix Hecto
            => new(1E2, "hecto", "h");

        /// <summary>
        /// deca: 1E1
        /// </summary>
        public static SIPrefix Deca
            => new(1E1, "deca", "da");

        /// <summary>
        /// deci: 1E-1
        /// </summary>
        public static SIPrefix Deci
            => new(1E-1, "deci", "d");

        /// <summary>
        /// centi: 1E-2
        /// </summary>
        public static SIPrefix Centi
            => new(1E-2, "centi", "c");

        /// <summary>
        /// milli: 1E-3
        /// </summary>
        public static SIPrefix Milli
            => new(1E-3, "milli", "m");

        /// <summary>
        /// micro: 1E-6
        /// </summary>
        public static SIPrefix Micro
            => new(1E-6, "micro", "µ");

        /// <summary>
        /// nano: 1E-9
        /// </summary>
        public static SIPrefix Nano
            => new(1E-9, "nano", "n");

        /// <summary>
        /// pico: 1E-12
        /// </summary>
        public static SIPrefix Pico
            => new(1E-12, "pico", "p");

        /// <summary>
        /// femto: 1E-15
        /// </summary>
        public static SIPrefix Femto
            => new(1E-15, "femto", "f");

        /// <summary>
        /// atto: 1E-18
        /// </summary>
        public static SIPrefix Atto
            => new(1E-18, "atto", "a");

        /// <summary>
        /// zepto: 1E-21
        /// </summary>
        public static SIPrefix Zepto
            => new(1E-21, "zepto", "z");

        /// <summary>
        /// yocto: 1E-24
        /// </summary>
        public static SIPrefix Yocto
            => new(1E-24, "yocto", "y");

        #endregion
    }

}