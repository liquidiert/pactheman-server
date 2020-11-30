import redis
import hashlib


class RedisController(object):

    __instance = None
    client: redis.Redis = None

    def __new__(cls):
        if RedisController.__instance is None:
            RedisController.__instance = object.__new__(cls)
            RedisController.__instance.client = redis.Redis('localhost', decode_responses=True)
        return RedisController.__instance

    def get(self, key):
        el = RedisController.__instance.client.get(key)
        return el if el is not None else ""

    def set(self, key, data):
        return RedisController.__instance.client.set(key, data)

    def hget(self, key):
        el = RedisController.__instance.client.hget(key)
        return el if el is not None else ""

    def hset(self, key, data: dict):
        return RedisController.__instance.client.hset(key, data)

    def lpush(self, key, data):
        return RedisController.__instance.client.lpush(key, data)

    def lrange(self, key, start=0, end=-1):
        return RedisController.__instance.client.lrange(key, start, end)