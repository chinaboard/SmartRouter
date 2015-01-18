# SmartRouter
快速修改OpenWrt的拨号密码

用校园宽带2天改一次密码，每次还要进后台，甚烦

于是就做了这么个玩意

# 用法
SmartRouter h 路由器IP ru 管理员 rp 管理员密码 pu pppoe账号 pp pppoe密码

# 样例
SmartRouter h 192.168.1.1 ru root rp root pu tyxy#18012345678 pp 123456

其中，路由器IP默认为192.168.1.1
管理员账号密码默认为root


# 注意
这货只在LuCI下能用

这货只能在http协议下使用

这货没有在别的地方用过，所以是纯自用的
