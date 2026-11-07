import sys
from pathlib import Path
from PIL import Image, ImageFilter

def enhance_image(input_path, output_path, scale_factor=2.5, unsharp_radius=2, unsharp_percent=150, unsharp_threshold=3):
    img = Image.open(input_path)
    # upscale
    new_size = (int(img.width * scale_factor), int(img.height * scale_factor))
    img_upscaled = img.resize(new_size, Image.Resampling.LANCZOS)
    # sharpen
    img_sharpened = img_upscaled.filter(ImageFilter.UnsharpMask(radius=unsharp_radius, percent=unsharp_percent, threshold=unsharp_threshold))
    img_sharpened.save(output_path)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python enhance_image.py <input_path> <output_path>")
        sys.exit(1)
    input_path = Path(sys.argv[1])
    output_path = Path(sys.argv[2])
    enhance_image(input_path, output_path)
