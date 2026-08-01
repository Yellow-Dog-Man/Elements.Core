namespace Elements.Core.Benchmark.Extensions;

public class FastNoiseBench
{
    private FastNoise Noise { get; set; } = new();

    [ParamsSource(nameof(Noises))]
    public FastNoise.NoiseType NoiseType { get; set; }
    public IEnumerable<FastNoise.NoiseType> Noises => [FastNoise.NoiseType.Cellular, FastNoise.NoiseType.Perlin,];

    [ParamsSource(nameof(Interps))]
    public FastNoise.Interp Interp { get; set; } = FastNoise.Interp.Linear;
    public IEnumerable<FastNoise.Interp> Interps =>
        [FastNoise.Interp.Linear, FastNoise.Interp.Hermite, FastNoise.Interp.Quintic];

    [IterationSetup]
    public void IterationSetup()
    {
        Noise.SetNoiseType(NoiseType);
        Noise.SetInterp(Interp);
    }

    [Benchmark]
    [Arguments(0, 0)]
    [Arguments(3.1431f, 2.875f)]
    [Arguments(314.31f, 287.5f)]
    public float Generate(float x, float y)
        => Noise.GetNoise(x, y);
}