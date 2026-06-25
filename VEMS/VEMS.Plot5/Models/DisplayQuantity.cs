namespace VEMS.Plot5
{

    /// <summary>
    /// number quantity tuple
    /// </summary>
    public struct NumberQuantity
    {
        /// <summary>
        /// display name of the number
        /// </summary>
        public string Name {  get; set; }

        /// <summary>
        /// value of the number
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// unit of the value
        /// [Not Implemented]
        /// </summary>
        public double Unit { get; set; }

    }


    /// <summary>
    /// text quantity tuple
    /// </summary>
    public struct TextQuantity
    {
        /// <summary>
        /// display name of the text
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// content of the text
        /// </summary>
        public string Value { get; set; }
        //public string Content { get; set; }

        /// <summary>
        /// size of the text
        /// </summary>
        public double Unit {  get; set; }
        //public double Size { get; set; }
    }


}
