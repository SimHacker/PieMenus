########################################################################
# SimCity Pie Menus.
# By Don Hopkins.


########################################################################


from piemenu import *
import gtk


########################################################################


def main():


    ########################################################################
    # Make Window and PieMenuTarget.


    win = gtk.Window()
    win.set_title("SimCity Pie Menus, by Don Hopkins")

    target = PieMenuTarget(label="SimCity Pie Menus")

    win.add(target)


    ########################################################################
    # Make zone_pie.


    zone_pie = PieMenu(
        header="Zone",
        fixed_radius=50,
        neutral_description="Select a zoning tool.")

    zone_pie.add_item(
        PieItem(
            description='Commercial\nzoning tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/iccom.png',
            icon_hilite='images/iccomhi.png'))

    zone_pie.add_item(
        PieItem(
            description='Industrial\nzoning tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icind.png',
            icon_hilite='images/icindhi.png'))

    zone_pie.add_item(
        PieItem(
            description='Police station\nzoning tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icpol.png',
            icon_hilite='images/icpolhi.png'))

    zone_pie.add_item(
        PieItem(
            description='Query zone\ntool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icqry.png',
            icon_hilite='images/icqryhi.png'))

    zone_pie.add_item(
        PieItem(
            description='Fire station\nzoning tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icfire.png',
            icon_hilite='images/icfirehi.png'))

    zone_pie.add_item(
        PieItem(
            description='Residential\nzoning tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icres.png',
            icon_hilite='images/icreshi.png'))

    ########################################################################
    # Make build_pie.


    build_pie = PieMenu(
        header="Build",
        fixed_radius=50,
        neutral_description="Select a building tool.")

    build_pie.add_item(
        PieItem(
            description='Park building tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icpark.png',
            icon_hilite='images/icparkhi.png'))

    build_pie.add_item(
        PieItem(
            description='Seaport building tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icseap.png',
            icon_hilite='images/icseaphi.png'))

    build_pie.add_item(
        PieItem(
            description='Nuclear power plant\nbuilding tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icnuc.png',
            icon_hilite='images/icnuchi.png'))

    build_pie.add_item(
        PieItem(
            description='Airport building tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icairp.png',
            icon_hilite='images/icairphi.png'))

    build_pie.add_item(
        PieItem(
            description='Coal power plant\nbuilding tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/iccoal.png',
            icon_hilite='images/iccoalhi.png'))

    build_pie.add_item(
        PieItem(
            description='Stadium building tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icstad.png',
            icon_hilite='images/icstadhi.png'))


    ########################################################################
    # Make tool_pie.


    tool_pie = PieMenu(
        header="SimCity Tools",
        fixed_radius=50,
        neutral_description="Select a SimCity editing tool,\nor the zone or build submenu.")

    tool_pie.add_item(
        PieItem(
            label='Zone...',
            description="Submenu of zoning tools.",
            lolite_fill_color=None,
            lolite_stroke_color=None,
            sub_pie=zone_pie))

    tool_pie.add_item(
        PieItem(
            description='Bulldozer editing tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icdozr.png',
            icon_hilite='images/icdozrhi.png'))

    tool_pie.add_item(
        PieItem(
            description='Road editing tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icroad.png',
            icon_hilite='images/icroadhi.png'))

    tool_pie.add_item(
        PieItem(
            description='Eraser drawing tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icersr.png',
            icon_hilite='images/icersrhi.png'))

    tool_pie.add_item(
        PieItem(
            label='Build...',
            description='Submenu of building tools.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            sub_pie=build_pie))

    tool_pie.add_item(
        PieItem(
            description='Chalk drawing tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icchlk.png',
            icon_hilite='images/icchlkhi.png'))

    tool_pie.add_item(
        PieItem(
            description='Rail editing tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icrail.png',
            icon_hilite='images/icrailhi.png'))

    tool_pie.add_item(
        PieItem(
            description='Wire editing tool.',
            lolite_fill_color=None,
            lolite_stroke_color=None,
            icon='images/icwire.png',
            icon_hilite='images/icwirehi.png'))


    ########################################################################


    target.setPie(tool_pie)

    win.resize(300, 300)

    win.show_all()

    gtk.main()


########################################################################


if __name__ == '__main__':
    main()


########################################################################
