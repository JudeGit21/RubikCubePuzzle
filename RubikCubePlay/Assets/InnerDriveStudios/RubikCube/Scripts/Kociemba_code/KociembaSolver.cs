using Kociemba;

public static class KociembaSolver
{
    public static string Solve(string cubeString, out string info)
    {
        return Search.solution(
            cubeString,
            out info,
            maxDepth: 22,
            timeOut: 6000,
            useSeparator: false
        );
    }
}
