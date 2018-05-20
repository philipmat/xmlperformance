# XML parsing performance in various languages/libraries

Inspired by another project I contribute to, [discogs-xml2db](https://github.com/philipmat/discogs-xml2db),
which struggles a bit parsing very large XML files, this project attempts to examine
the parsing performance of XML in various languages and libraries.

There are two tests, both dealing with [Discogs](https://www.discogs.com/)
[monthly data dumps](https://data.discogs.com/):

- Parsing labels dump, a little over 200MB in size;
- Parsing artists dump, about 1.3GB worth of data.

For the purpose of reducing the size of the repo, the actual dumps are not included,
but they are freely available from the
[Discogs monthly data dumps](https://data.discogs.com/) site.

## TL;DR Results

Tests performed on an desktop computer with:

- CPU: Intel i5-2500K @ 3.30GHz
- RAM: 16GB
- Disk: Samsung SSD 850 Pro 512GB
- OS: Windows 10 Pro 64-bit (version 1803 / build 17134.48)


Methodology:

- Fresh run for each language after reboot;
- Capture average of ?? runs
- Execution time captured using language libraries to remove start-up time.

| Language/Library | Labels Dump (~200MB) | Artists Dump (1.27GB) |
|------------------|----------------------|-----------------------|
| .Net Core        |                      |                       |
| .Net Standard    |                      |                       |
| Python - xml.sax |                      |                       |
| Node - ??        |                      |                       |



## Python

### xml.sax

## .Net

### .Net Core

### .Net Standard
