using System.Threading.Tasks;
using Kociemba;

public static class KociembaSolver
{
    // Run solver off the main thread so Unity does not freeze.
    public static Task<(string solution, string info)> SolveAsync(string cubeString, int maxDepth = 21, long timeOut = 10, bool useSeparator = false)
    {
        return Task.Run(() =>
        {
            string info;
            string sol = Search.solution(cubeString, out info, maxDepth: maxDepth, timeOut: timeOut, useSeparator: useSeparator);
            return (sol, info);
        });
    }
}
