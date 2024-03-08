from flask import Flask, render_template, request, jsonify
import os
import datetime

app = Flask(__name__)

data_list = []


@app.route('/')
def index():
  return render_template('index.html', data_list=data_list)


@app.route('/api/add_data', methods=['POST'])
def add_data():
  data = {
      'ip': request.remote_addr,
      'computer_name': request.headers.get('Host'),
      'time': datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
      'file_path': handle_file_upload(request)
  }
  data_list.append(data)
  return jsonify({'success': True})


def handle_file_upload(request):
  file = request.files.get('file')
  if file:
    file_path = os.path.join('uploads', file.filename)
    file.save(file_path)
    return file_path
  return None


if __name__ == '__main__':
  os.makedirs('uploads', exist_ok=True)
  app.run(host='0.0.0.0', port=8080)
