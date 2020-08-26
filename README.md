# MP±1
Quickly convert between media formats (eg, mp3 to mp4 etc)

# How to use
There is a mappings.txt file, you can *map* one file type to another. When converting, it will look for the file type in the mappings and automatically convert to the specified type.

You are free to either open the application directly and use the menus or drag the file to be converted into the application. Keep in mind that you should always wait a little bit after conversion for the files to be fully written.

# Troubleshooting:
## I can't find my exported files
ensure there's an "Export" folder in the same folder as the mppm1 application

## It says something about not having ffmpeg
1) download the ffmpeg library from https://www.ffmpeg.org/
2) unzip the contents somewhere
3) copy the "ffmpeg.exe" file and place it in the same folder as the mppm1 application
4) try again

## My computer is super slow/I can't delete certain files in the exports folder
you've probably closed the program whilst it was converting a large file
1) open task manager (CTRL+SHIFT+ESC)
2) kill the mppm.exe process
