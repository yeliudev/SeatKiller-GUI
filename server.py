# Copyright (c) Ye Liu. All rights reserved.

import smtplib
from datetime import datetime
from email.header import Header
from email.mime.text import MIMEText
from socketserver import BaseRequestHandler, ThreadingTCPServer

SMTP_SERVER = 'smtp-mail.outlook.com'
FROM_ADDR = 'seatkiller@outlook.com'
EMAIL_PASSWD = 'skiller@root'


class SocketHandler(BaseRequestHandler):

    def send_email(self, data, to_addr):
        try:
            body = '-----------------------------------------------------'
            body += '\nID：%d' % data['id']
            body += '\n凭证号码：%s' % data['receipt']
            body += '\n时间：%s %s～%s' % (data['onDate'], data['begin'],
                                       data['end'])
            body += '\n状态：%s' % ('已签到' if data['checkedIn'] else '预约')
            body += '\n地址：%s' % data['location']
            body += '\n-----------------------------------------------------'
            body += '\n\nPowered by SeatKiller'

            msg = MIMEText(body, 'plain', 'utf-8')
            msg['From'] = 'SeatKiller <%s>' % FROM_ADDR
            msg['To'] = 'User <%s>' % to_addr
            msg['Subject'] = Header('座位预约成功', 'utf-8').encode()

            server = smtplib.SMTP(SMTP_SERVER, 587)
            server.starttls()
            server.login(FROM_ADDR, EMAIL_PASSWD)
            server.sendmail(FROM_ADDR, to_addr, msg.as_string())
            server.quit()

            return True
        except Exception:
            return False

    def handle(self):
        try:
            self.request.sendall('hello'.encode())
            data = self.request.recv(512).decode()
            info = data.split()
            if info[0] == 'notice':
                return
            if info[0] == 'login':
                time_str = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
                print(f'\n{time_str} {info[1]} {info[2]} ({info[3]})')
            elif info[0] == 'json':
                json = eval(data[5:])
                print('\n%s' % data[5:])
                print('\nSending mail to %s...' % json['to_addr'], end='')

                if self.send_email(json['data'], json['to_addr']):
                    self.request.sendall('succeed'.encode())
                    print('succeed')
                else:
                    self.request.sendall('fail'.encode())
                    print('failed')
            else:
                print('\nFormat error: %s' % data)
        except Exception:
            pass


if __name__ == '__main__':
    server = ThreadingTCPServer(('0.0.0.0', 5210), SocketHandler)
    print('Waiting for connection...')
    server.serve_forever()
