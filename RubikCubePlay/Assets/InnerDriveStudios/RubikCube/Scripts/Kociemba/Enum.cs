namespace Kociemba
{
    /**
    * <pre>
    * The names of the facelet positions of the cube
    *             ||
    *             |*U1U2U3|
    *             ||
    *             |*U4U5U6|
    *             ||
    *             |*U7U8U9|
    *             ||
    * ||||
    * *L1L2L3|F1F2F3|R1R2F3|B1B2B3|
    * ||||
    * L4L5L6|F4F5F6|R4R5R6|B4B5B6|
    * ||||
    * L7L8L9|F7F8F9|R7R8R9|B7B8B9|
    * ||||
    *             ||
    *             |*D1D2D3|
    *             ||
    *             |*D4D5D6|
    *             ||
    *             |*D7D8D9|
    *             |*****|
    * </pre>
    *
    *A cube definition string "UBL..." means for example: In position U1 we have the U-color, in position U2 we have the
    * B-color, in position U3 we have the L color etc. according to the order U1, U2, U3, U4, U5, U6, U7, U8, U9, R1, R2,
    * R3, R4, R5, R6, R7, R8, R9, F1, F2, F3, F4, F5, F6, F7, F8, F9, D1, D2, D3, D4, D5, D6, D7, D8, D9, L1, L2, L3, L4,
    * L5, L6, L7, L8, L9, B1, B2, B3, B4, B5, B6, B7, B8, B9 of the enum constants.
*/
    public enum Facelet
    {
        U1, U2, U3, U4, U5, U6, U7, U8, U9, R1, R2, R3, R4, R5, R6, R7, R8, R9, F1, F2, F3, F4, F5, F6, F7, F8, F9, D1, D2, D3, D4, D5, D6, D7, D8, D9, L1, L2, L3, L4, L5, L6, L7, L8, L9, B1, B2, B3, B4, B5, B6, B7, B8, B9
    }
    //++++++++++++++++++++++++++++++ Names the colors of the cube facelets ++++++++++++++++++++++++++++++++++++++++++++++++
    public enum CubeColor
    {
        U, R, F, D, L, B
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //The names of the corner positions of the cube. Corner URF e.g., has an U(p), a R(ight) and a F(ront) facelet
    public enum Corner
    {
        URF, UFL, ULB, UBR, DFR, DLF, DBL, DRB
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //Then names of the edge positions of the cube. Edge UR e.g., has an U(p) and R(ight) facelet.
    public enum Edge
    {
        UR, UF, UL, UB, DR, DF, DL, DB, FR, FL, BL, BR
    }
}