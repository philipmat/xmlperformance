import xml.sax.handler
from models import Label, Artist
from typing import List


class BaseHandler(xml.sax.handler.ContentHandler):
    __text_buffer = ''
    __current_node: str = None  # type: str
    __current_model = None

    def __init__(
                self,
                stats_collector,
                main_node: str,
                text_nodes: List[str],
                ignore_nodes: List[str],
                model_prop_map):

        self._stats_collector = stats_collector

        self._main_node = main_node
        self._text_nodes = text_nodes
        self._ignore_nodes = ignore_nodes
        self._model_prop_map = model_prop_map

    def startElement(self, name, attrs):  # noqa: N802
        # if we're currently inside an ignore node, we'll skip this start node
        if self.__current_node in self._ignore_nodes:
            self.__text_buffer = ''
            return

        self.__current_node = name
        if name == self._main_node:
            self.__current_model = self._new_model()
            self.__text_buffer = ''
            return

    def endElement(self, name):  # noqa: N802
        # if name is in _ignore_nodes, it means we're done ignoring
        if name in self._ignore_nodes:
            self.__current_node = None
            self.__text_buffer = ''
            return

        # we're inside an ignore node, we don't care about processing
        if self.__current_node in self._ignore_nodes:
            self.__text_buffer = ''
            return

        # if it's the end of the main node, let's collect it
        if name == self._main_node:
            self._stats_collector.collect(self.__current_model)
            self.__text_buffer = ''
            return

        # it means we're in a regular node
        self.__text_buffer = self.__text_buffer.strip()
        self._set_text(self.__current_model, name, self.__text_buffer)
        self.__text_buffer = ''

    def characters(self, data):
        self.__text_buffer += data

    def _new_model(self):
        raise NotImplementedError()

    def _set_text(self, model, node: str, text: str, parent_node: str = None) -> None:
        if node in self._model_prop_map:
            self._model_prop_map[node](model, text)

    @staticmethod
    def _set_id(model, value):
        model.id = int(value)


class LabelsHandler(BaseHandler):
    def __init__(self, stats_collector):
        autos = ['name', 'profile', 'data_quality']
        prop_map = {p: lambda model, value: setattr(model, p, value) for p in autos}
        prop_map['id'] = BaseHandler._set_id
        prop_map['url'] = lambda model, value: model.urls.append(value)
        super().__init__(
            stats_collector,
            main_node='label',
            text_nodes=["id", "name", "contactInfo", "profile", "data_quality", "url"],
            ignore_nodes=['images', 'sublabels'],
            model_prop_map=prop_map)

    def _new_model(self):
        return Label()


class ArtistsHandler(BaseHandler):
    def __init__(self, stats_collector):
        autos = ['name', 'profile', 'data_quality']
        prop_map = {p: lambda model, value: setattr(model, p, value) for p in autos}
        prop_map['id'] = BaseHandler._set_id
        prop_map['realname'] = lambda model, value: setattr(model, 'real_name', value)
        prop_map['url'] = lambda model, value: model.urls.append(value)
        super().__init__(
            stats_collector,
            main_node='artist',
            text_nodes=["id", "name", "contactInfo", "profile", "data_quality", "url"],
            ignore_nodes=['images', 'namevariations', 'aliases', 'groups', 'members'],
            model_prop_map=prop_map)

    def _new_model(self):
        return Artist()
