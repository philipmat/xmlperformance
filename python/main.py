import gzip
import time
import stats
import sys
from handlers import LabelsHandler, ArtistsHandler
from xml.sax import make_parser


def make_handler(file: str):
    if 'labels' in file:
        return LabelsHandler
    if 'artists' in file:
        return ArtistsHandler
    return None


def parse(file: str, parser) -> None:
    _open = open
    if file.endswith(".gz"):
        _open = gzip.open
    with _open(file, encoding="utf8") as f:
        parser.parse(f)


def time_format(time: float):
    multiplier, ms = (1, "s") if time > 10 else (1000, "ms")
    return f"{time * multiplier:,.0f}{ms}"


def main(argv):
    file = argv[0]
    collector = stats.StatsCollector()
    handler = make_handler(file)
    if handler is None:
        print(f"I don't know how to handle {file}.")
        sys.exit(2)

    parser = make_parser()
    parser.setContentHandler(handler(collector))
    ts = time.time()
    parse(file, parser)
    elapsed = time.time() - ts
    print(str(collector))
    print(f"Parsing of {file} took {time_format(elapsed)} and used ?? bytes of memory.")


if __name__ == "__main__":
    main(sys.argv[1:])
