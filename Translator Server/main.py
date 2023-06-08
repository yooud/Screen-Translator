from deep_translator import GoogleTranslator
from aiohttp.web import *
import requests
import logging
import json
import os


TESSDATA_DIR = '/tessdata'
TESSDATA_URL = 'https://api.github.com/repos/tesseract-ocr/tessdata/contents'
TESSDATA = []

if not os.path.exists(TESSDATA_DIR):
    os.mkdir(TESSDATA_DIR)

async def translate(request: Request):
    content: dict = await request.json()
    if 'text' not in content.keys():
        return Response(status=400)

    try:
        translated = GoogleTranslator(source=str(content.get('src', 'auto')), target=str(content.get('dest', 'en')))\
            .translate(text=str(content.get('text')))
    except Exception as e:
        logging.info(f'Wrong content: {content}')
        print(e)
        return Response(status=500)

    content['text'] = translated
    return json_response(content)


async def languages(request: Request):
    return json_response(list(GoogleTranslator().get_supported_languages(as_dict=True).values()))


async def tessdata(request: Request):
    data = request.match_info.get('data', None)
    if data:
        path = os.path.join(TESSDATA_DIR, f'{data}.traineddata')
        if os.path.exists(path):
            return FileResponse(path=path)
        else:
            return Response(status=400)
    else:
        return json_response(TESSDATA)


def update_tessdata():
    text: str = "Downloading tessdata - {} ({}/{})"
    r = requests.get(TESSDATA_URL)
    content: dict = json.loads(r.content)
    urls: list = []
    for data in content:
        if data['name'].endswith('.traineddata') and len(data['name']) == 15 and data['name'] != 'osd.traineddata':
            TESSDATA.append(data['name'].split('.traineddata')[0])
            urls.append(data['download_url'])

    for data in TESSDATA:
        if not os.path.exists(os.path.join(TESSDATA_DIR, f"{data}.traineddata")):
            os.system('cls')
            print(text.format(data, TESSDATA.index(data), len(TESSDATA)))
            r = requests.get(urls[TESSDATA.index(data)])
            with open(os.path.join(TESSDATA_DIR, f"{data}.traineddata"), 'wb') as file:
                file.write(r.content)


if __name__ == '__main__':
    logging.basicConfig(level=logging.INFO)
    update_tessdata()
    app = Application()
    app.add_routes([post('/api/translate', translate),
                    post('/api/languages', languages),
                    post('/api/tessdata', tessdata),
                    post('/api/tessdata/{data}', tessdata)])
    run_app(app, port=7878)