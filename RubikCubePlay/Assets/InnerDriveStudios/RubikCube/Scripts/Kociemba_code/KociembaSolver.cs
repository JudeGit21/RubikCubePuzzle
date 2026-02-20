using System.Threading.Tasks;
using Kociemba;

public static class KociembaSolver
{
    // Synkron variant (passar din Thread-lösning i RubikFaceScanner)
    public static string Solve(string cubeString, out string info, int maxDepth = 21, long timeOut = 10, bool useSeparator = false)
    {
        return Search.solution(cubeString, out info, maxDepth: maxDepth, timeOut: timeOut, useSeparator: useSeparator);
    }

    // Async variant (om du vill köra utan egen Thread)
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