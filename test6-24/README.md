# test6-24 — VEMS Simulation Suite

**Date:** 2024-06-24
**Platform:** VEMS (Visual Electromagnetic Simulator)
**Engine:** .NET 8.0 + Intel MKL 2025.3

## Simulations

### SIM 1: FreeSpace SPW 1D Propagation
- **Algorithm:** Angular Spectrum of Plane Waves (SPW)
- **Wavelength:** 632.8 nm (He-Ne laser)
- **Input:** Plane wave, 75 μm aperture with 7.5 μm Gaussian edge
- **Propagation distance:** 675 μm
- **Computation time:** ~21 ms
- **Result:** Energy conservation ratio ~1.094

### SIM 2: RCWA 1D Rectangular Grating Diffraction
- **Algorithm:** Rigorous Coupled-Wave Analysis (RCWA)
- **Wavelength:** 193 nm (DUV lithography)
- **Grating period:** 25.456 μm, ridge width: 12.728 μm
- **Thickness:** 100 nm, TM polarization
- **Materials:** n_in=1.5593, n_ridge=1.3282+i1.6637, n_out=1.0
- **Computation time:** ~5.4 s (S-matrix)
- **Validation:** Analytical vs FFT-sampled medium comparison

## Output Files

```
code/
├── Program.cs           # Simulation source code
└── VEMS.SimRunner.csproj
output/
├── data/
│   ├── spw1d_propagation.csv   # SPW field data (x, |E|_in, |E|_out)
│   └── rcwa1d_grating.csv      # RCWA diffraction data (kx, |E|, phase)
└── images/
    ├── spw1d_propagation.png        # SPW input vs output field
    ├── rcwa1d_grating.png           # RCWA transmitted field |E| vs kx
    └── rcwa1d_grating_profile.png   # Grating refractive index profile
```
